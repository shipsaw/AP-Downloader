param (
    $dbPath = "$PSScriptRoot\ProductsDb.db",
    $exportPath = "$PSScriptRoot\productsCSV.csv"
)
$db = New-SqliteConnection $dbPath
Invoke-SqliteQuery -DataSource $dbPath -Query "select P.ProductID, P.Name, P.Filename, P.ImageName as 'Preview Image', BP.Filename as 'Branding Patch', ES.Filename as 'Extra Stock', LP.Filename as 'Livery Pack' from Product AS P
left outer join BrandingPatch BP on P.ProductID = BP.ProductID
left outer join ExtraStock ES on P.ProductID = ES.ProductID
left outer join LiveryPack LP on P.ProductID = LP.ProductID" |
export-csv $exportPath -NoTypeInformation