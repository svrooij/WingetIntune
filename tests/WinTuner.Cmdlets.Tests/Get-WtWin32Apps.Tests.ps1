
Describe 'Get-WtWin32Appsp' {
    It 'Should be available' {
        $cmdlet = Get-Command -Name 'Get-WtWin32Apps'
        $cmdlet.CommandType | Should -Be 'Cmdlet'
    }
}