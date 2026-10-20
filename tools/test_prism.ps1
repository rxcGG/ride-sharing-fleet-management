$ErrorActionPreference = 'Stop'
$base = 'http://127.0.0.1:4010'
$results = @()

$validBody = '{"passengerId":"928f4af3-33a8-4d81-9c08-f255540e4fd1","pickup":{"latitude":49.4229,"longitude":26.9871},"dropoff":{"latitude":49.4384,"longitude":26.9872},"fareEstimate":185.5}'
$valid = Invoke-WebRequest -Method Post -Uri "$base/trips" -ContentType 'application/json' -Body $validBody
$results += [pscustomobject]@{ name='Коректний запит'; method='POST'; path='/trips'; status=$valid.StatusCode; expected=201; response=$valid.Content }

$invalidBody = '{"passengerId":"bad","pickup":{"latitude":110,"longitude":26},"dropoff":{"latitude":49,"longitude":26},"fareEstimate":-1}'
$invalid = Invoke-WebRequest -Method Post -Uri "$base/trips" -ContentType 'application/json' -Body $invalidBody -SkipHttpErrorCheck
$invalidText = [Text.Encoding]::UTF8.GetString([byte[]]$invalid.Content)
$results += [pscustomobject]@{ name='Запит із помилковими даними'; method='POST'; path='/trips'; status=$invalid.StatusCode; expected=422; response=$invalidText }

$filtered = Invoke-WebRequest -Method Get -Uri "$base/trips?status=REQUESTED&limit=5&cursor=next-page"
$results += [pscustomobject]@{ name='Відфільтрований список'; method='GET'; path='/trips?status=REQUESTED&limit=5&cursor=next-page'; status=$filtered.StatusCode; expected=200; response=$filtered.Content }

$destination = Join-Path $PSScriptRoot '..\docs\evidence\prism-tests.json'
$results | ConvertTo-Json -Depth 8 | Set-Content -Path $destination -Encoding utf8
$results | ForEach-Object { "{0}: {1} {2}, отримано {3}, очікувалось {4}" -f $_.name,$_.method,$_.path,$_.status,$_.expected }
if ($results.Where({ $_.status -ne $_.expected }).Count -gt 0) { exit 1 }
