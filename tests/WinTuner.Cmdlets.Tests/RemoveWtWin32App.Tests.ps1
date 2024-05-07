

Describe 'Remove-WtWin32App' {
    It 'Should be available' {
        $cmdlet = Get-Command -Name 'Remove-WtWin32App'
        $cmdlet.CommandType | Should -Be 'Cmdlet'
    }
}