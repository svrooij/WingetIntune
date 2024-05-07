# BeforeAll {
#   Import-Module WinTuner -Force
  
# }

BeforeDiscovery {
  $commands = Get-Command -Module WinTuner | Select-Object -ExpandProperty Name
}

Describe "WinTuner Module tests" {

  It "should have at least 8 commands" {
    Get-Command -Module WinTuner | Should -HaveCount 8
  }

  Context "Command <_>" -ForEach $commands {
    It "should have a help URI" {
      $command = Get-Command -Name $_
      $command.HelpUri | Should -Not -BeNullOrEmpty
    }
  }

}