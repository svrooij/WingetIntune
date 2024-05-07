BeforeAll {
    Import-Module ./dist/WinTuner/WinTuner.psd1
}

Describe 'New-IntuneWinPackage' {
    It 'Should be available' {
        $cmdlet = Get-Command -Name 'New-IntuneWinPackage'
        $cmdlet.CommandType | Should -Be 'Cmdlet'
    }
}