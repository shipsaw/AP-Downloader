package main

import (
	"bufio"
	"fmt"
	"golang.org/x/sys/windows"
	"log"
	"os"
)

func main() {
	// Check for correct permissions, un-comment this for release build
	//if !checkElevatedStatus() {
	//	log.Fatal("Error getting correct user permissions")
	//}
	manifest, err := os.Open(`C:\ProgramData\ApDownloader\Downloads.txt`)
	if err != nil {
		log.Fatal("Error reading install manifest")
	}
	defer manifest.Close()
	scanner := bufio.NewScanner(manifest)
	installLoc := getInstallLocation(scanner)
	for scanner.Scan() {
		fmt.Println(scanner.Text())
	}
	fmt.Println("Railwords install folder: " + installLoc)
}

func getInstallLocation(scanner *bufio.Scanner) string {
	scanner.Scan()
	if err := scanner.Err(); err != nil {
		log.Fatal("No Railworks folder path provided")
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
		log.Fatalf("SID Error: %s", err)
		return false
	}

	// This appears to cast a null pointer so I'm not sure why this
	// works, but this guy says it does and it Works for Me™:
	// https://github.com/golang/go/issues/28804#issuecomment-438838144
	token := windows.Token(0)

	member, err := token.IsMember(sid)
	if err != nil {
		log.Fatalf("Token Membership Error: %s", err)
		return false
	}

	// Also note that an admin is _not_ necessarily considered
	// elevated.
	// For elevation see https://github.com/mozey/run-as-admin
	if token.IsElevated() && member {
		return true
	}
	return false
}
