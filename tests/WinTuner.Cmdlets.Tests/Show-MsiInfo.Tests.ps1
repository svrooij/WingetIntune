Describe 'Show-MsiInfo' {
    It 'Should be available' {
        $cmdlet = Get-Command -Name 'Show-MsiInfo'
        $cmdlet.CommandType | Should -Be 'Cmdlet'
    }

    It 'Should download and show MSI info' {
        $msiUrl = 'https://download.microsoft.com/download/C/7/A/C7AAD914-A8A6-4904-88A1-29E657445D03/LAPS.x64.msi'
        $tempFolder = $env:TEMP
        $msiInfo = $(Show-MsiInfo -MsiUrl $msiUrl -OutputPath $tempFolder)
        $msiInfo | Should -Not -BeNullOrEmpty
        $msiInfo.ProductCode | Should -Be '{97E2CA7B-B657-4FF7-A6DB-30ECC73E1E28}'
        $msiInfo.ProductVersion | Should -Be '6.2.0.0'
    }
}