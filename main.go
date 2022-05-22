package main

import (
	"archive/zip"
	"bufio"
	"bytes"
	"fmt"
	"golang.org/x/sys/windows"
	"io"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
)

func main() {
	fmt.Println("Starting")
	//Check for correct permissions, un-comment this for release build
	//if !checkElevatedStatus() {
	//	fmt.Println("Error getting correct user permissions")
	//	//fmt.Println("Error getting correct user permissions")
	//}
	manifest, err := os.Open(`C:\ProgramData\ApDownloader\Downloads.txt`)
	if err != nil {
		//fmt.Println("Error reading install manifest")
		fmt.Println("Error reading install manifest")
	}
	defer manifest.Close()
	fmt.Println("Install manifest read successfully")
	fmt.Println("Getting install folder")

	scanner := bufio.NewScanner(manifest)
	installLoc := getInstallLocation(scanner)
	fmt.Println("Install folder retrieved")
	fmt.Println("Railworks install folder: " + installLoc + "\n")

	var setupExes []string
	for scanner.Scan() {
		setupExes = append(setupExes, scanner.Text())
	}
	fmt.Printf("There are %d addons to install\n", len(setupExes))

	for _, f := range setupExes {
		path := unzipAddon(f)
		installAddon(path)
	}

	fmt.Println("Complete")
	fmt.Scanln()

}

func unzipAddon(fileName string) string {
	installerExe := ""
	reader, err := zip.OpenReader(fileName)
	if err != nil {
		fmt.Println("Error unzipping files")
	}
	defer reader.Close()

	for _, f := range reader.File {
		if filepath.Ext(f.Name) == ".exe" {
			installerExe = f.Name
		}
		err = unzipFile(f, `C:\temp`)
		if err != nil {
			fmt.Println("Error unzipping ", f)
		}
	}
	return filepath.Join("C:\\temp\\" + installerExe)
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

func installAddon(setupExe string) {
	installCmd := `& '` + setupExe + `' /b"C:\temp" /s /v"/qn INSTALLDIR="C:\Railworks" /L+i "C:\temp\install.log""`
	fmt.Print("Installing " + filepath.Base(setupExe) + " ... ")
	cmd := exec.Command("powershell", "-NoProfile", "-NonInteractive", installCmd)

	var stdout bytes.Buffer
	var stderr bytes.Buffer
	cmd.Stdout = &stdout
	cmd.Stderr = &stderr

	err := cmd.Run()
	if err != nil {
		fmt.Println(err)
	}
	if stdout.Len() > 0 {
		fmt.Println("\nStdOut: " + stdout.String())
	} else if stderr.Len() > 0 {
		fmt.Println("\nStdErr: " + stderr.String())
	} else {
		fmt.Println("Done")
	}
}

func getInstallLocation(scanner *bufio.Scanner) string {
	scanner.Scan()
	if err := scanner.Err(); err != nil {
		fmt.Println("No Railworks folder path provided")
	}
	return scanner.Text()

}

func checkElevatedStatus() bool {
	var sid *windows.SID
	// Although this looks scary, it is directly copied from the
	// official windows documentation. The Go API for this is a
	// direct wrap around the official C++ API.
	// See https://docs.microsoft.com/en-us/windows/desktop/api/securitybaseapi/nf-securitybaseapi-checktokenmembership
	err := windows.AllocateAndInitializeSid(
		&windows.SECURITY_NT_AUTHORITY,
		2,
		windows.SECURITY_BUILTIN_DOMAIN_RID,
		windows.DOMAIN_ALIAS_RID_ADMINS,
		0, 0, 0, 0, 0, 0,
		&sid)
	if err != nil {
		//fmt.Printlnf("SID Error: %s", err)
		fmt.Println("SID Error: %s", err)
		return false
	}

	// This appears to cast a null pointer so I'm not sure why this
	// works, but this guy says it does and it Works for Me™:
	// https://github.com/golang/go/issues/28804#issuecomment-438838144
	token := windows.Token(0)

	member, err := token.IsMember(sid)
	if err != nil {
		//fmt.Printlnf("Token Membership Error: %s", err)
		fmt.Println("Token Membership Error: %s", err)
		return false
	}

	// Also note that an admin is _not_ necessarily considered
	// elevated.
	// For elevation see https://github.com/mozey/run-as-admin
	if token.IsElevated() && member {
		fmt.Println(token.IsElevated(), member)
		return true
	}
	fmt.Println(token.IsElevated(), member)
	return false
}
