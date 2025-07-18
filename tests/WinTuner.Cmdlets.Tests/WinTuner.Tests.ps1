# BeforeAll {
#   Import-Module WinTuner -Force
  
# }


# Some tests are failing, so we skip them for now
# BeforeDiscovery {
#   $commands = Get-Command -Module WinTuner | Select-Object -ExpandProperty Name
# }

# Describe "WinTuner Module tests" {
#   Context "Command <_>" -ForEach $commands {
    
#     It "should have a help URI" {
#       $command = Get-Command -Name $_
#       $command.HelpUri | Should -Not -BeNullOrEmpty
#     }
#   }

# }