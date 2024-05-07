BeforeAll {
    Import-Module ./dist/WinTuner/WinTuner.psd1
}

Describe 'Remove-WtWin32App' {
    It 'Should be available' {
        $cmdlet = Get-Command -Name 'Remove-WtWin32App'
        $cmdlet.CommandType | Should -Be 'Cmdlet'
    }
}