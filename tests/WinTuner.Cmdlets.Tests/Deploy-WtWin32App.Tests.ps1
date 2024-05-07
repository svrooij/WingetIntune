
Describe 'Deploy-WtWin32App' {
    It 'Should be available' {
        $cmdlet = Get-Command -Name 'Deploy-WtWin32App'
        $cmdlet.CommandType | Should -Be 'Cmdlet'
    }
}