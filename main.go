package main

import (
	"bufio"
	"fmt"
	"golang.org/x/sys/windows"
	"os"
	"os/exec"
)

func main() {
	fmt.Println("Starting")
	//Check for correct permissions, un-comment this for release build
	//if !checkElevatedStatus() {
	//	fmt.Println("Error getting correct user permissions")
	//	//fmt.Println("Error getting correct user permissions")
	//}
	fmt.Scanln()
	manifest, err := os.Open(`C:\ProgramData\ApDownloader\Downloads.txt`)
	if err != nil {
		fmt.Scanln()
		//fmt.Println("Error reading install manifest")
		fmt.Println("Error reading install manifest")
	}
	defer manifest.Close()
	fmt.Println("Install manifest read successfully")
	fmt.Scanln()
	fmt.Println("Getting install folder")

	scanner := bufio.NewScanner(manifest)
	installLoc := getInstallLocation(scanner)
	fmt.Println("Install folder retrieved")
	for scanner.Scan() {
		fmt.Println(scanner.Text())
	}
	fmt.Println("Railworks install folder: " + installLoc)
	fmt.Scanln()
	//args := "/b\"C:\\temp\" /S /v\"/qn INSTALLDIR=\"C:\\Railworks\"\""
	//program := `JTA-JUA-PTA Wagon Pack.exe`
	//args := []string{`/s`, `/b"C:\temp"`, `/v"/qn"`} //" INSTALLDIR="C:\Railwords"`}
	fmt.Println("Begin cmd")
	fmt.Scanln()
	exec.Command("powershell", "-NoProfile", `& 'C:\JTA-JUA-PTA Wagon Pack.exe' /b"C:\temp" /s /v"/qn INSTALLDIR="C:\Railwords""`).Run()
	if err != nil {
		fmt.Println(err)
	}
	fmt.Println("Complete")
	fmt.Scanln()

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
