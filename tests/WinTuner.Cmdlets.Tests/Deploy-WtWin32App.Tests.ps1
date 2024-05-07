BeforeAll {
    Import-Module ./dist/WinTuner/WinTuner.psd1
}

Describe 'Deploy-WtWin32App' {
    It 'Should be available' {
        $cmdlet = Get-Command -Name 'Deploy-WtWin32App'
        $cmdlet.CommandType | Should -Be 'Cmdlet'
    }
}