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
	zipLoc         string
}

//goland:noinspection ALL
func main() {
	fmt.Println("AP Install Manager v1.3\n******************\n")

	appManifestFile := readArguments(os.Args)
	userDirs, setupZips, manifestErrors := readManifest(appManifestFile)
	userDirs = copy7zToExecLoc(userDirs)
	defer os.RemoveAll(userDirs.tempDir)
	defer os.RemoveAll(userDirs.zipLoc)
	installsFailed, installsSucceeded := installAddons(setupZips, userDirs)
	userLogErrors := generateUserLogs(userDirs, installsFailed, installsSucceeded)
	// If there are non-fatal errors, exit with exit code, but have the calling application check to see if some
	// Installations succeeded
	log.Printf("\n\nFinished Execution\n******************\n\n")
	fmt.Println("Complete...")
	time.Sleep(2 * time.Second)
	if userLogErrors != nil || manifestErrors != nil {
		os.Exit(1)
	}
}

func copy7zToExecLoc(userDirs userDirectories) userDirectories {
	temp7zDir, err := os.MkdirTemp("", "temp7zDir")
	new7zFile := filepath.Join(temp7zDir, "7za.exe")

	existing7z, err := os.Open(userDirs.zipLoc)
	if err != nil {
		log.Fatal("Unable to copy 7z: " + err.Error())
	}
	defer existing7z.Close()

	new7z, err := os.Create(new7zFile)
	if err != nil {
		log.Fatal("Unable to copy 7z: " + err.Error())
	}
	defer new7z.Close()

	_, err = io.Copy(new7z, existing7z)
	if err != nil {
		log.Fatal("Unable to copy 7z: " + err.Error())
	}

	retUserDirs := userDirs
	retUserDirs.zipLoc = new7zFile
	return retUserDirs
}

// Gets the file that contains files to install and folder locations
// Any problems here are fatal as a problem here is unrecoverable
func readArguments(args []string) string {
	if len(args) != 2 {
		log.Fatal("Error: must provide argument with location of manifest file")
	}
	appManifestFile := string(args[1])
	file, err := openProgramLogFile(filepath.Join(filepath.Dir(appManifestFile), "programLog.log"))
	if err != nil {
		log.Fatal(err)
	}
	log.SetOutput(file)
	log.SetFlags(log.LstdFlags | log.Lshortfile | log.Lmicroseconds)
	log.Printf("\n\n*******************\nBeginning Execution\n\n")
	return appManifestFile
}

// Looks through the manifest to get install zips and folder locations
// As with the previous function a failure at this point is unrecoverable
func readManifest(appManifestFile string) (userDirectories, []string, error) {
	manifestNonFatalErrors := false
	tempDir, err := os.MkdirTemp("", "apDownloader")
	if err != nil {
		log.Fatal("Error, Unable to create temporary directory; exiting")
	}

	successLogPath := filepath.Join(filepath.Dir(appManifestFile), `logSuccess.log`)
	failureLogPath := filepath.Join(filepath.Dir(appManifestFile), `logFailure.log`)
	successLogExists, ExistsErr1 := exists(successLogPath)
	failureLogExists, ExistsErr2 := exists(failureLogPath)
	if ExistsErr1 != nil || ExistsErr2 != nil {
		log.Fatal("Unexpected Error attempting to clean up logs")
	}
	if successLogExists {
		err = os.Remove(filepath.Join(filepath.Dir(appManifestFile), `logSuccess.log`))
		if err != nil {
			log.Println("Unable to remove old success log")
			manifestNonFatalErrors = true
		}
	}
	if failureLogExists {
		err = os.Remove(filepath.Join(filepath.Dir(appManifestFile), `logFailure.log`))
		if err != nil {
			log.Println("Unable to remove old failure log")
			manifestNonFatalErrors = true
		}
	}

	manifest, err := os.Open(appManifestFile)
	if err != nil {
		log.Fatal("Error reading install manifest")
	}
	defer manifest.Close()

	scanner := bufio.NewScanner(manifest)
	userDirs := getUserDirs(scanner)
	userDirs.tempDir = tempDir
	userDirs.programDataDir = filepath.Dir(appManifestFile)
	log.Println("Railworks install folder: " + userDirs.installDir)
	log.Println("Temp dir: " + userDirs.tempDir)
	log.Println("ProgramDataDir dir: " + userDirs.programDataDir)
	log.Println("Download Dir: " + userDirs.downloadDir + "\n")

	var setupZips []string
	for scanner.Scan() {
		setupZips = append(setupZips, scanner.Text())
	}
	if manifestNonFatalErrors {
		return userDirs, setupZips, errors.New("error removing old log files")
	}
	return userDirs, setupZips, nil
}

// Installs exe and rwp addons, any failures will be added to the failed addon log
// but the installation should continue if possible
// returns list of failed and successful installs/unzips
func installAddons(setupZips []string, userDirs userDirectories) ([]string, []string) {
	var installsFailed []string
	var installsSucceeded []string

	totalFiles := len(setupZips)
	for i, f := range setupZips {
		path, err := unzipAddon(f, userDirs)
		if err != nil {
			log.Println("Unable to extract " + path + ": " + err.Error())
			installsFailed = append(installsFailed, trimPath(f))
			continue
		}
		if filepath.Ext(path) == ".exe" {
			path, err := installExeAddon(path, i+1, totalFiles, userDirs)
			if err != nil {
				log.Printf("%s failed to execute in powershell with error: %", path, err)
				installsFailed = append(installsFailed, trimPath(path))
				continue
			}
		} else if filepath.Ext(path) == ".rwp" {
			err = unzipRWP(path, i+1, totalFiles, userDirs)
			if err != nil {
				log.Println("Unable to extract RWP file: " + path + " to " + userDirs.installDir)
				installsFailed = append(installsFailed, trimPath(path))
				continue
			}
			// Unzipping success won't be seen in the powershell logs, so we add it here
			installsSucceeded = append(installsSucceeded, trimPath(path))
		}
	}
	return installsFailed, installsSucceeded
}

// Returns path of unzipped file
func unzipAddon(fileName string, userDirs userDirectories) (string, error) {
	installerExe := ""
	reader, err := zip.OpenReader(fileName)
	if err != nil {
		return "", fmt.Errorf("unable to unzip %s, error: %s", fileName, err.Error())
	}
	defer reader.Close()

	for _, f := range reader.File {
		if filepath.Ext(f.Name) == ".exe" || filepath.Ext(f.Name) == ".rwp" {
			installerExe = f.Name
			err = unzipFile(f, userDirs.tempDir)
			if err != nil {
				backupSetupExe, err := unzipBackupMethod(fileName, userDirs)
				if err != nil {
					return "", fmt.Errorf("unable to unzip %s: %s", f.Name, err.Error())
				}
				return backupSetupExe, nil
			}
		}
	}
	if installerExe == "" {
		log.Println("No valid install files in zip: " + fileName)
		return fileName, fmt.Errorf("no valid install files in zip")
	}
	return filepath.Join(userDirs.tempDir, installerExe), nil
}

// Attempt unzip with 7zip directly
func unzipBackupMethod(fileName string, userDirs userDirectories) (exe string, retErr error) {
	defer func() {
		if err := recover(); err != nil {
			log.Println("Panic occurred:", err)
			exe = ""
			retErr = errors.New(fmt.Sprintf("Unable to Extract %s: %s", fileName, err))
		}
	}()
	log.Println("Attempting backup unzip method for " + fileName)
	sevenZipExists, err := exists(filepath.Join(userDirs.zipLoc))
	if err != nil {
		log.Println("error attempting to locate 7zip: " + err.Error())
		return "", errors.New("7z executable not found")
	} else if !sevenZipExists {
		log.Println("Error: 7-zip does not exist in " + userDirs.zipLoc)
		return "", errors.New("7z executable not found")
	}

	log.Printf("Generating command: %s %s -aoa -y -o %s\n", userDirs.zipLoc, fileName, userDirs.tempDir)
	cmd := exec.Command(userDirs.zipLoc, "x", fileName, "-aoa", "-y", "-o"+userDirs.tempDir)
	log.Println("Generated, running command ", cmd.String())
	err = cmd.Run()
	log.Println("Complete...")
	if err != nil {
		return "", err
	}
	log.Println("Locating exe file in " + userDirs.tempDir)
	setupExe, err := filepath.Glob(filepath.Join(userDirs.tempDir, "*.exe"))
	if err != nil {
		return "", err
	}
	if len(setupExe) == 1 {
		return setupExe[0], nil
	}
	return "", errors.New("Unable to extract using backup method: " + fileName + ": " + err.Error())
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

	sevenZipExists, err := exists(filepath.Join(userDirs.zipLoc))
	if err != nil {
		log.Println("error attempting to locate 7zip: " + err.Error())
		return errors.New("7z executable not found")
	} else if !sevenZipExists {
		log.Println("Error: 7-zip does not exist in " + userDirs.zipLoc)
		return errors.New("7z executable not found")
	}

	cmd := exec.Command(userDirs.zipLoc, "x", fileName, "-aoa", "-y", "-o"+userDirs.installDir)
	err = cmd.Run()
	if err != nil {
		log.Println(err)
	}

	s.Stop()
	fmt.Println(installingText + "... Done")
	log.Println(installingText + "... Done")
	return nil
}

// Installs a zip with a setup.exe.  If installation fails before powershell takes over we return the failed
// path, otherwise we collect that information from the powershell logs
func installExeAddon(setupExe string, progress int, totalFlies int, userDirs userDirectories) (string, error) {
	installingText := fmt.Sprintf("Installing %d/%d: %s", progress, totalFlies, filepath.Base(setupExe))
	s := spinner.New(spinner.CharSets[26], 400*time.Millisecond) // Build our new spinner
	s.Prefix = installingText
	s.Start() // Start the spinner

	logFile := filepath.Join(userDirs.tempDir, "install.log")
	installDir := fmt.Sprintf("/qn INSTALLDIR=\"%s\" /L+I %s", userDirs.installDir, logFile)
	cmd := exec.Command(setupExe, "/b"+userDirs.tempDir, "/s", "/v"+installDir)

	var stdout bytes.Buffer
	var stderr bytes.Buffer
	cmd.Stdout = &stdout
	cmd.Stderr = &stderr

	err := cmd.Run()
	if err != nil {
		return setupExe, err
	}
	s.Stop()

	err = cleanupTempFolders(setupExe, userDirs)
	if err != nil {
		log.Println("... Errors cleaning up GUID temp folders")
		fmt.Println(installingText + "... Errors")
		return setupExe, err
	}

	fmt.Println(installingText + "... Done")
	log.Println(installingText + "... Done")
	return "", nil
}

// returns installation directory, download directory
// Fatal fail as error is unrecoverable
func getUserDirs(scanner *bufio.Scanner) userDirectories {
	scanner.Scan()
	if err := scanner.Err(); err != nil {
		log.Fatal("No 7zip folder path provided")
	}
	zipLoc := scanner.Text()

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
		zipLoc:      zipLoc,
	}
}

func DecodeUTF16(b []byte) (string, error) {

	if len(b)%2 != 0 {
		return "", fmt.Errorf("must have even length byte slice")
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
	canCreateSuccessfulLogFile := createUserLogFile(successfulInstalls, filepath.Join(downloadDir, "logSuccess.log"))
	canCreateFailedLogFile := createUserLogFile(failedInstalls, filepath.Join(downloadDir, "logFailure.log"))

	if !canCreateSuccessfulLogFile || !canCreateFailedLogFile {
		errMsg := ""
		if !canCreateFailedLogFile {
			errMsg += "Unable to create log of failed installations\n"
		}
		if !canCreateSuccessfulLogFile {
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
			if strings.Contains(line, "Product:") {
				line = strings.Split(line, "Product: ")[1]
				line = strings.Split(line, " --")[0]
			}
			_, err = file.WriteString(line + "\n")
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

// Generates log to assist debugging as well as reporting ot the main app which installations have succeeded/failed
func generateUserLogs(userDirs userDirectories, installsFailed []string, installsSucceeded []string) error {
	errorInLogging := false
	installLog, err := os.ReadFile(filepath.Join(userDirs.tempDir, `install.log`))
	if err != nil {
		log.Println("Installation log not found, unable to generate reports")
		errorInLogging = true
	}

	cleanLog, err := DecodeUTF16(installLog)
	if err != nil {
		log.Println("Error decoding the UTF16 log file: " + err.Error())
		errorInLogging = true
	}
	stringLines := strings.Split(cleanLog, "\n")

	for _, line := range stringLines {
		if strings.Contains(line, "Product:") && strings.Contains(line, "successfully") {
			installsSucceeded = append(installsSucceeded, line)
		} else if strings.Contains(line, "Product:") && strings.Contains(line, "failed") {
			installsFailed = append(installsFailed, line)
		}
	}

	err = createUserLogFiles(installsSucceeded, installsFailed, userDirs.programDataDir)
	if err != nil {
		log.Println(err)
		errorInLogging = true
	}

	if errorInLogging {
		return errors.New("there was a problem generating the logs")
	}
	return nil
}

func cleanupTempFolders(setupExe string, userDirs userDirectories) error {
	errorsInCleanup := false

	err := os.Remove(setupExe)
	if err != nil {
		log.Println("Unable to clean up setup EXE file: " + setupExe)
		errorsInCleanup = true
	}

	appDataTemp := filepath.Join(filepath.Dir(userDirs.tempDir), "{*}")
	tempGuids, err := filepath.Glob(appDataTemp)
	if err != nil {
		log.Println("Unable to clean up temp GUID-named files")
		errorsInCleanup = true
	}
	for _, f := range tempGuids {
		fileInfo, err := os.Stat(f)
		if err != nil {
			log.Println("Unable to inspect file " + f + " for deletion")
			errorsInCleanup = true
		}
		if time.Now().Sub(fileInfo.ModTime()) < 3*time.Minute {
			err = os.RemoveAll(f)
			if err != nil {
				log.Println("Error in removing the TempGUID" + err.Error())
				errorsInCleanup = true
			}
		}
	}
	if errorsInCleanup {
		return errors.New("there was a problem cleaning up the temporary folders")
	}
	return nil
}

// exists returns whether the given file or directory exists
func exists(path string) (bool, error) {
	_, err := os.Stat(path)
	if err == nil {
		return true, nil
	}
	if os.IsNotExist(err) {
		return false, nil
	}
	return false, err
}

func trimPath(path string) string {
	return strings.TrimSuffix(filepath.Base(path), filepath.Ext(path))
}
