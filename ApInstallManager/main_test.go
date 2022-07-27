package installManager

import (
	"testing"
)

func TestTrim_EmptyStringReturnsEmpty(t *testing.T) {
	// Arrange
	testString := ""
	expected := "."

	// Act
	actual := trimPath(testString)

	// Assert
	if expected != actual {
		t.Errorf("Expected: <%s>, Actual: <%s>", expected, actual)
	}
}

func TestTrim_ExtensionOnly(t *testing.T) {
	// Arrange
	testString := "TestFile.zip"
	expected := "TestFile"

	// Act
	actual := trimPath(testString)

	// Assert
	if expected != actual {
		t.Errorf("Expected: <%s>, Actual: <%s>", expected, actual)
	}
}

func TestTrim_FullPath(t *testing.T) {
	// Arrange
	testString := "C:/Manager/TestFile.zip"
	expected := "TestFile"

	// Act
	actual := trimPath(testString)

	// Assert
	if expected != actual {
		t.Errorf("Expected: <%s>, Actual: <%s>", expected, actual)
	}
}

func TestCopy(t *testing.T) {
	mockDirectories := userDirectories{
		installDir:     "testFS/installdir",
		downloadDir:    "testFS/downloaddir",
		programDataDir: "testFS/programdata",
		tempDir:        "temp",
		zipLoc:         "testFS/zip/7z.exe",
		tempDirPath:    "testFS/",
	}
	returnDirs := copy7zToExecLoc(mockDirectories)
	if returnDirs.installDir == "" {
		t.Error("Invalid return directories")
	}
}
