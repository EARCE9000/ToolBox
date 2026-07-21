# ScreenAnnotation Standalone Publish Script

$ErrorActionPreference = "Continue"
Set-Location $PSScriptRoot

try {
	Write-Host "===================================================" -ForegroundColor Cyan
	Write-Host " ScreenAnnotation Standalone Publish Started..." -ForegroundColor Cyan
	Write-Host "===================================================" -ForegroundColor Cyan
	Write-Host ""

	$profiles = @(
		@{ Name = "Win-Arm64-Standalone"; Display = "[1/2] Win-Arm64-Standalone" },
		@{ Name = "Win-x64-Standalone"; Display = "[2/2] Win-x64-Standalone" }
	)

	foreach ($profile in $profiles) {
		Write-Host "$($profile.Display) Publishing..." -ForegroundColor Yellow
		dotnet publish /p:PublishProfile=$($profile.Name) /nologo

		if ($LASTEXITCODE -ne 0) {
			Write-Host "  Error: Failed to publish $($profile.Name)" -ForegroundColor Red
		} else {
			Write-Host "  Success" -ForegroundColor Green
		}
		Write-Host ""
	}

	Write-Host "===================================================" -ForegroundColor Cyan
	Write-Host " Copying documentation to distribution folder..." -ForegroundColor Cyan
	Write-Host "===================================================" -ForegroundColor Cyan

	$publishRoot = "\\yog-sothoth.yggdrasill.earce.net\yama\Workshop_System\opt_rpa\codesign\10.Unsigned\ScreenAnnotation"

	# Files to copy
	$filesToCopy = @(
		"publish.md",
		"ArrowIcon.png",
		"ArrowIconButton.png",
		"DrawSpeechBubble.png",
		"DrawSpeechBubbleButton.png",
		"ExitButton.png",
		"ClearButton.png",
		"SaveButton.png"
	)

	foreach ($file in $filesToCopy) {
		$sourcePath = Join-Path $PSScriptRoot $file
		$destPath = Join-Path $publishRoot $file

		try {
			if (Test-Path $sourcePath) {
				Copy-Item -Path $sourcePath -Destination $destPath -Force -ErrorAction Stop
				Write-Host "Success: $file copied" -ForegroundColor Green
			} else {
				Write-Host "Warning: $file not found, skipped" -ForegroundColor Yellow
			}
		} catch {
			Write-Host "Error: Failed to copy $file" -ForegroundColor Red
			Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
		}
	}

	Write-Host ""
	Write-Host "===================================================" -ForegroundColor Cyan
	Write-Host " All publish operations completed!" -ForegroundColor Cyan
	Write-Host "===================================================" -ForegroundColor Cyan
}
catch {
	Write-Host ""
	Write-Host "Unexpected error occurred:" -ForegroundColor Red
	Write-Host $_.Exception.Message -ForegroundColor Red
}
finally {
	Write-Host ""
	Write-Host "Press Enter to continue..."
	$null = Read-Host
}
