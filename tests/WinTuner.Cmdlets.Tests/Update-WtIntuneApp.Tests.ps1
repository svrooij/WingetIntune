
Describe 'Update-WtIntuneApp' {
    It 'Should be available' {
        $cmdlet = Get-Command -Name 'Update-WtIntuneApp'
        $cmdlet.CommandType | Should -Be 'Cmdlet'
    }
}