Describe 'WinTunerProxy' {
  It 'Should respond within 15 seconds' {
      $measurement = Measure-Command -Expression { $response = Invoke-WebRequest -Uri https://proxy.wintuner.app -ConnectionTimeoutSeconds 30; $response.StatusCode | Should -Be 200 }
      $measurement.TotalSeconds | Should -BeLessThan 15
  }

  It 'Should produce a valid json at /api/swagger.json' {
      $response = Invoke-WebRequest -Uri https://proxy.wintuner.app/api/swagger.json
      $response.StatusCode | Should -Be 200
      $response.Content | ConvertFrom-Json | Should -Not -BeNullOrEmpty
  }

  Describe 'Package' {
    It 'Should return package information for "9NZVDKPMR9RD"' {
        $response = Invoke-RestMethod -Uri https://proxy.wintuner.app/api/store/package/9NZVDKPMR9RD -Headers @{ 'x-functions-key' = $env:WINTUNER_PROXY_CODE }
        $response.packageIdentifier | Should -Be '9NZVDKPMR9RD'
        $response.displayName | Should -Be 'Mozilla Firefox'
        $response.publisher | Should -Be 'Mozilla'

        $response.description | Should -Not -BeNullOrEmpty
        $response.iconUrl | Should -Not -BeNullOrEmpty
        $response.informationUrl | Should -Not -BeNullOrEmpty
        $response.privacyInformationUrl | Should -Not -BeNullOrEmpty
        $response.scope | Should -BeIn 'system', 'user'
    }

    It 'Should return package information for "XP9KHM4BK9FZ7Q"' {
      $response = Invoke-RestMethod -Uri https://proxy.wintuner.app/api/store/package/XP9KHM4BK9FZ7Q -Headers @{ 'x-functions-key' = $env:WINTUNER_PROXY_CODE }
      $response.packageIdentifier | Should -Be 'XP9KHM4BK9FZ7Q'
      $response.displayName | Should -Be 'Visual Studio Code'
      $response.publisher | Should -Be 'Microsoft Corporation'

      $response.description | Should -Not -BeNullOrEmpty
      $response.iconUrl | Should -Not -BeNullOrEmpty
      $response.informationUrl | Should -Not -BeNullOrEmpty
      $response.privacyInformationUrl | Should -Not -BeNullOrEmpty
      $response.scope | Should -BeIn 'system', 'user'
    }
  }

  Describe 'Search' {
    It 'Should find packageIdentifier for "Visual Studio Code"' {
        $response = Invoke-RestMethod -Uri https://proxy.wintuner.app/api/store/search?searchString=Visual+Studio+Code -Headers @{ 'x-functions-key' = $env:WINTUNER_PROXY_CODE }
        # The response looks like:
        # [{"packageIdentifier": "string","displayName": "string","publisher": "string"}]
        # and is valid JSON with an array of objects
        $response | Should -HaveCount 2
        $item = $response[1];
        $item.packageIdentifier | Should -Be 'XP9KHM4BK9FZ7Q'
    }
  }
}