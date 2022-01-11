module ApDownloader.Core.DataAccess

open Microsoft.Data.Sqlite
open FSharp.Data.Dapper

let private dbConnectionString (dataSource: string) = $"Data Source = %s{dataSource};"

let productDbConnection () =
    new SqliteConnection(dbConnectionString "./ProductsDb.db")

let settingsDbConnection () =
    new SqliteConnection(dbConnectionString "./Settings.db")

let productDbF () =
    SqliteConnection(productDbConnection ())

let settingsDbF () =
    SqliteConnection(settingsDbConnection ())
//let querySeqAsync<'R> = querySeqAsync<'R> (productDbF)
//let querySingleAsync<'R> = querySingleOptionAsync<'R> (productDbF)

[<CLIMutable>]
type DbProduct =
    { ProductId: int
      Name: string
      Filename: string
      ImageName: string
      EsFilename: string
      BpFilename: string
      LpFilename: string }

type Product =
    { ProductId: int
      Name: string
      Filename: string
      ImageName: string
      EsFilename: string option
      BpFilename: string option
      LpFilename: string option }

[<CLIMutable>]
type Config =
    { GetExtraStock: bool
      GetBrandingPatch: bool
      GetLiveryPack: bool
      DownloadFilepath: string
      InstallFilepath: string }

let GetAllBaseProducts =
    querySeqAsync<DbProduct> (productDbF) { script "SELECT * FROM Product" }

let ToFunctionalOption (dbProduct: DbProduct) : Product =
    let EsOption =
        match dbProduct.EsFilename with
        | null -> None
        | _ -> Some dbProduct.EsFilename

    let BpOption =
        match dbProduct.BpFilename with
        | null -> None
        | _ -> Some dbProduct.BpFilename

    let LpOption =
        match dbProduct.LpFilename with
        | null -> None
        | _ -> Some dbProduct.LpFilename

    { ProductId = dbProduct.ProductId
      Name = dbProduct.Name
      Filename = dbProduct.Filename
      ImageName = dbProduct.ImageName
      EsFilename = EsOption
      BpFilename = BpOption
      LpFilename = LpOption }

let GetAllProductsOption =
    let GetAllProducts =
        querySeqAsync<DbProduct> (productDbF) {
            script
                "SELECT Name, Product.Filename, ImageName,
        ES.Filename AS EsFilename,
        BP.Filename AS BpFilename,
        LP.Filename AS LpFilename FROM Product
        LEFT JOIN ExtraStock AS ES ON ES.ProductID = Product.ProductID
        LEFT JOIN BrandingPatch AS BP ON BP.ProductID = Product.ProductID
        LEFT JOIN LiveryPack AS LP ON LP.ProductID = Product.ProductID"
        }
    // Remove the nulls
    let OptionProducts =
        GetAllProducts
        |> Async.RunSynchronously
        |> Seq.cast<DbProduct>
        |> Seq.map (ToFunctionalOption)

    OptionProducts

let GetConfig =
    querySingleAsync<Config> (settingsDbF) {
        script
            "SELECT GetExtraStock, GetBrandingPatch, GetLiveryPack, DownloadFilepath, InstallFilepath
        FROM Settings"
    }
    |> Async.RunSynchronously

let UpdateConfig newConfig =
    querySingleOptionAsync<int> (settingsDbF) {
        script
            "UPDATE Settings
        SET GetExtraStock = @GetExtraStock, GetBrandingPatch = @GetBrandingPatch, GetLiveryPack = @GetLiveryPack,
        DownloadFilepath = @DownloadFilepath, InstallFilepath = @InstallFilepath"

        parameters (
            dict [ "GetExtraStock", box newConfig.GetExtraStock
                   "GetBrandingPatch", box newConfig.GetBrandingPatch
                   "GetLiveryPack", box newConfig.GetLiveryPack
                   "DownloadFilepath", box newConfig.DownloadFilepath
                   "InstallFilepath", box newConfig.InstallFilepath ]
        )
    }
    |> Async.RunSynchronously
