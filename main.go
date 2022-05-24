package main

import (
	"archive/zip"
	"bufio"
	"bytes"
	"errors"
	"fmt"
	"github.com/briandowns/spinner"
	"io"
	"log"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"time"
	"unicode/utf16"
	"unicode/utf8"
)

type userDirectories struct {
	installDir     string
	downloadDir    string
	programDataDir string
	tempDir        string
}

func main() {
	fmt.Println("AP Install Manager\n******************\n")

	if len(os.Args) != 2 {
		log.Fatal("Error: must provide argument with location of manifest file")
	}
	appManifestFile := string(os.Args[1])

	file, err := openProgramLogFile(filepath.Join(filepath.Dir(appManifestFile), "programLog.log"))
	if err != nil {
		log.Fatal(err)
	}
	log.SetOutput(file)
	log.SetFlags(log.LstdFlags | log.Lshortfile | log.Lmicroseconds)
	log.Println("Beginning Execution")

	tempDir, err := os.MkdirTemp("", "apDownloader")
	if err != nil {
		log.Fatal("Error, Unable to create temporary directory; exiting")
	}
	defer os.Remove(tempDir)
	os.Remove(filepath.Join(tempDir, `install.log`))
	os.Remove(filepath.Join(filepath.Dir(appManifestFile), `logSuccess.log`))
	os.Remove(filepath.Join(filepath.Dir(appManifestFile), `logFailure.log`))

	manifest, err := os.Open(appManifestFile)
	if err != nil {
		log.Fatal("Error reading install manifest")
	}
	defer manifest.Close()

	scanner := bufio.NewScanner(manifest)
	userDirs := getUserDirs(scanner)
	userDirs.tempDir = tempDir
	userDirs.programDataDir = filepath.Dir(appManifestFile)
	fmt.Println("Railworks install folder: " + userDirs.installDir + "\n")

	var setupExes []string
	for scanner.Scan() {
		setupExes = append(setupExes, scanner.Text())
	}
	totalFiles := len(setupExes)

	for i, f := range setupExes {
		path, err := unzipAddon(f, tempDir)
		if err != nil {
			// TODO: Append failed file unzips
		}
		if filepath.Ext(path) == ".exe" {
			installAddon(path, i+1, totalFiles, tempDir, userDirs.installDir)
		} else if filepath.Ext(path) == ".rwp" {
			err = unzipRWP(path, i+1, totalFiles, userDirs)
			if err != nil {
				log.Println("Unable to extract RWP file: " + path + " to " + userDirs.installDir)
			}
		}
	}

	installLog, err := os.ReadFile(filepath.Join(tempDir, `install.log`))
	if err != nil {
		log.Println("Installation log not found, unable to generate reports")
	}

	cleanLog, err := DecodeUTF16(installLog)
	stringLines := strings.Split(cleanLog, "\n")

	var successfulInstalls []string
	var failedInstalls []string
	for _, line := range stringLines {
		if strings.Contains(line, "Product:") && strings.Contains(line, "successfully") {
			successfulInstalls = append(successfulInstalls, line)
		} else if strings.Contains(line, "Product:") && strings.Contains(line, "failed") {
			failedInstalls = append(failedInstalls, line)
		}
	}

	err = createUserLogFiles(successfulInstalls, failedInstalls, userDirs.programDataDir)
	if err != nil {
		log.Fatal(err)
	}

	fmt.Println("Complete...")
	time.Sleep(2 * time.Second)
	log.Println("Execution complete")
}

// Returns path of unzipped file
func unzipAddon(fileName string, tempDir string) (string, error) {
	installerExe := ""
	reader, err := zip.OpenReader(fileName)
	if err != nil {
		return "", fmt.Errorf("Unable to unzip %s", fileName)
	}
	defer reader.Close()

	for _, f := range reader.File {
		if filepath.Ext(f.Name) == ".exe" || filepath.Ext(f.Name) == ".rwp" {
			installerExe = f.Name
			err = unzipFile(f, tempDir)
			if err != nil {
				return "", fmt.Errorf("Unable to unzip %s", f.Name)
			}
		}
	}
	return filepath.Join(tempDir, installerExe), nil
}

func unzipFile(f *zip.File, destination string) error {
	// 4. Check if file paths are not vulnerable to Zip Slip
	filePath := filepath.Join(destination, f.Name)
	if !strings.HasPrefix(filePath, filepath.Clean(destination)+string(os.PathSeparator)) {
		return fmt.Errorf("invalid file path: %s", filePath)
	}

	// 5. Create directory tree
	if f.FileInfo().IsDir() {
		if err := os.MkdirAll(filePath, os.ModePerm); err != nil {
			return err
		}
		return nil
	}

	if err := os.MkdirAll(filepath.Dir(filePath), os.ModePerm); err != nil {
		return err
	}

	// 6. Create a destination file for unzipped content
	destinationFile, err := os.OpenFile(filePath, os.O_WRONLY|os.O_CREATE|os.O_TRUNC, f.Mode())
	if err != nil {
		return err
	}
	defer destinationFile.Close()

	// 7. Unzip the content of a file and copy it to the destination file
	zippedFile, err := f.Open()
	if err != nil {
		return err
	}
	defer zippedFile.Close()

	if _, err := io.Copy(destinationFile, zippedFile); err != nil {
		return err
	}
	return nil
}

func unzipRWP(fileName string, progress int, totalFiles int, userDirs userDirectories) error {
	installingText := fmt.Sprintf("Installing %d/%d: %s", progress, totalFiles, filepath.Base(fileName))
	s := spinner.New(spinner.CharSets[26], 400*time.Millisecond) // Build our new spinner
	s.Prefix = installingText
	s.Start() // Start the spinner

	unzipCommand := fmt.Sprintf("& ./7z.exe x \"%s\" -aoa -y -o\"%s\"", fileName, userDirs.installDir)
	cmd := exec.Command("powershell", "-NoProfile", "-NonInteractive", unzipCommand)
	err := cmd.Run()
	if err != nil {
		fmt.Println(err)
	}

	s.Stop()
	fmt.Println(installingText + "... Done")
	log.Println(installingText + "... Done")
	return nil
}

func installAddon(setupExe string, progress int, totalFlies int, tempDir string, installDir string) {
	installCmd := fmt.Sprintf("& '%s' /b\"%s\" /s /v\"/qn INSTALLDIR=\"%s\" /L+i \"%s\"\"",
		setupExe, tempDir, installDir, filepath.Join(tempDir, `install.log`))
	installingText := fmt.Sprintf("Installing %d/%d: %s", progress, totalFlies, filepath.Base(setupExe))
	s := spinner.New(spinner.CharSets[26], 400*time.Millisecond) // Build our new spinner
	s.Prefix = installingText
	s.Start() // Start the spinner
	cmd := exec.Command("powershell", "-NoProfile", "-NonInteractive", installCmd)

	var stdout bytes.Buffer
	var stderr bytes.Buffer
	cmd.Stdout = &stdout
	cmd.Stderr = &stderr

	err := cmd.Run()
	if err != nil {
		fmt.Println(err)
	}
	s.Stop()

	if stdout.Len() > 0 {
		log.Println("\nStdOut: " + stdout.String())
	} else if stderr.Len() > 0 {
		log.Println("\nStdErr: " + stderr.String())
	} else {
		fmt.Println(installingText + "... Done")
		log.Println(installingText + "... Done")
	}
}

// returns installation directory, download directory
func getUserDirs(scanner *bufio.Scanner) userDirectories {
	scanner.Scan()
	if err := scanner.Err(); err != nil {
		log.Fatal("No Railworks folder path provided")
	}
	installDir := scanner.Text()

	scanner.Scan()
	if err := scanner.Err(); err != nil {
		log.Fatal("No Railworks folder path provided")
	}
	downloadDir := scanner.Text()

	return userDirectories{
		installDir:  installDir,
		downloadDir: downloadDir,
	}
}

func DecodeUTF16(b []byte) (string, error) {

	if len(b)%2 != 0 {
		return "", fmt.Errorf("Must have even length byte slice")
	}

	u16s := make([]uint16, 1)

	ret := &bytes.Buffer{}

	b8buf := make([]byte, 4)

	lb := len(b)
	for i := 0; i < lb; i += 2 {
		u16s[0] = uint16(b[i]) + (uint16(b[i+1]) << 8)
		r := utf16.Decode(u16s)
		n := utf8.EncodeRune(b8buf, r[0])
		ret.Write(b8buf[:n])
	}

	return ret.String(), nil
}

func createUserLogFiles(successfulInstalls []string, failedInstalls []string, downloadDir string) error {
	canCreateSuc := createUserLogFile(successfulInstalls, filepath.Join(downloadDir, "logSuccess.log"))
	canCreateFail := createUserLogFile(failedInstalls, filepath.Join(downloadDir, "logFailure.log"))

	if !canCreateFail || !canCreateSuc {
		errMsg := ""
		if !canCreateFail {
			errMsg += "Unable to create log of failed installations\n"
		}
		if !canCreateSuc {
			errMsg += "Unable to create log of successful installations\n"
		}
		return errors.New(errMsg)
	} else {
		return nil
	}
}

func createUserLogFile(logLines []string, logPath string) bool {
	canCreate := true
	if logLines != nil {
		file, err := os.Create(logPath)
		if err != nil {
			canCreate = false
		}
		for _, line := range logLines {
			frontRemoved := strings.Split(line, "Product: ")[1]
			rearRemoved := strings.Split(frontRemoved, " --")[0]
			_, err = file.WriteString(rearRemoved + "\n")
			if err != nil {
				canCreate = false
			}
		}
	}
	return canCreate
}

func openProgramLogFile(path string) (*os.File, error) {
	logFile, err := os.OpenFile(path, os.O_WRONLY|os.O_APPEND|os.O_CREATE, 0644)
	if err != nil {
		return nil, err
	}
	return logFile, nil
}
