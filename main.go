package main

import (
	"archive/zip"
	"bufio"
	"bytes"
	"errors"
	"fmt"
	"github.com/briandowns/spinner"
	"io"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"time"
	"unicode/utf16"
	"unicode/utf8"
)

func main() {
	fmt.Println("Starting")
	tempDir, err := os.MkdirTemp("", "apDownloader")
	if err != nil {
		fmt.Println("Error, Unable to create temporary directory; exiting")
		fmt.Scanf("h")
		os.Exit(1)
	}
	defer os.Remove(tempDir)
	os.Remove(filepath.Join(tempDir, `install.log`))

	manifest, err := os.Open(`C:\ProgramData\ApDownloader\Downloads.txt`)
	if err != nil {
		fmt.Println("Error reading install manifest")
		fmt.Scanf("h")
		os.Exit(1)
	}
	defer manifest.Close()

	scanner := bufio.NewScanner(manifest)
	installLoc := getInstallLocation(scanner)
	fmt.Println("Railworks install folder: " + installLoc + "\n")
	downloadLoc := `C:\ProgramData\ApDownloader`

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
		installAddon(path, i+1, totalFiles, tempDir, installLoc)
	}

	installLog, err := os.ReadFile(filepath.Join(tempDir, `install.log`))
	if err != nil {
		fmt.Println("Installation log not found, unable to generate reports")
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

	err = createLogFiles(successfulInstalls, failedInstalls, downloadLoc)
	if err != nil {
		fmt.Println(err)
	}

	fmt.Println("Complete")
	fmt.Scanf("h")
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
		if filepath.Ext(f.Name) == ".exe" {
			installerExe = f.Name
		}
		err = unzipFile(f, tempDir)
		if err != nil {
			return "", fmt.Errorf("Unable to unzip %s", f.Name)
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
		fmt.Println("\nStdOut: " + stdout.String())
	} else if stderr.Len() > 0 {
		fmt.Println("\nStdErr: " + stderr.String())
	} else {
		fmt.Println(installingText + "... Done")
	}
}

func getInstallLocation(scanner *bufio.Scanner) string {
	scanner.Scan()
	if err := scanner.Err(); err != nil {
		fmt.Println("No Railworks folder path provided")
	}
	return scanner.Text()

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

func createLogFiles(successfulInstalls []string, failedInstalls []string, downloadDir string) error {
	canCreateSuc := true
	canCreateFail := true
	if successfulInstalls != nil {
		file, err := os.Create(filepath.Join(downloadDir, "logSuccess.log"))
		if err != nil {
			canCreateSuc = false
		}
		for _, line := range successfulInstalls {
			frontRemoved := strings.Split(line, "Product: ")[1]
			rearRemoved := strings.Split(frontRemoved, " --")[0]
			_, err = file.WriteString(rearRemoved + "\n")
			if err != nil {
				canCreateSuc = false
			}
		}
	}
	if failedInstalls != nil {
		file, err := os.Create(filepath.Join(downloadDir, "logFailed.log"))
		canCreateFail = false
		for _, line := range failedInstalls {
			frontRemoved := strings.Split(line, "Product: ")[1]
			rearRemoved := strings.Split(frontRemoved, " --")[0]
			_, err = file.WriteString(rearRemoved + "\n")
			if err != nil {
				canCreateFail = false
			}
		}
	}
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
