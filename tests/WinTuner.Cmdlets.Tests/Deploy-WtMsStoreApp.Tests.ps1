BeforeAll {
    Import-Module ./dist/WinTuner/WinTuner.psd1
}

Describe 'Deploy-WtMsStoreApp' {
    It 'Should be available' {
        $cmdlet = Get-Command -Name 'Deploy-WtMsStoreApp'
        $cmdlet.CommandType | Should -Be 'Cmdlet'
    }
}