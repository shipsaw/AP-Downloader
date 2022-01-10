module ApDownloader.Core.Project

open System
open System.Data
open System.IO
open System.Net.Http
open Elmish.WPF
open Domain
open Dapper.FSharp
open Microsoft.Data.Sqlite

let ProdDbConnectionString: IDbConnection =
    upcast new SqliteConnection(
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            Path.GetFileName(".ApDownloader/ProductsDb.db")
        )
    )

Dapper.FSharp.OptionTypes.register ()
let productTable = table<Product>

let init () =
    { CurrentPage = LoginPage
      AllApProducts = GetProductsOnly()
      PurchasedProducts = Seq.empty
      DownloadedProducts = Seq.empty
      DiscProducts = Seq.empty
      Client = None
      DlConfig =
          { GetExtraStock = true
            GetBrandingPatch = true
            GetLiveryPack = true
            DownloadFilepath = @"C:\"
            InstallFilepath = @"C:\Games" }
      ProductManifest =
          { ProductIds = Seq.empty
            ProductFilenames = Seq.empty
            ExtraStockFilenames = Seq.empty
            BrandingPatchesFilenames = Seq.empty
            LiveryPackFilenames = Seq.empty } }

type Msg =
    | SelLoginPage
    | SelDownloadPage
    | SelInstallPage
    | SelOptionsPage
    | Login of HttpClient
    | Download
    | ProductsOnDisc
    | Install
    | ApplyOptions
    | UpdateDb

let DownloadAddons = Seq.empty
let GetDownloadedAddons = Seq.empty
let GetProductsOnDisc = Seq.empty

let SetConfig =
    { GetExtraStock = true
      GetBrandingPatch = true
      GetLiveryPack = true
      DownloadFilepath = @"C:\"
      InstallFilepath = @"C:\" }

let update (msg: Msg) (model: Model) : Model =
    match msg with
    | SelLoginPage -> { model with CurrentPage = LoginPage }
    | SelDownloadPage -> { model with CurrentPage = DownloadPage }
    | SelInstallPage -> { model with CurrentPage = InstallPage }
    | SelOptionsPage -> { model with CurrentPage = OptionsPage }
    | Login client -> { model with Client = Some client }
    | Download -> { model with DownloadedProducts = DownloadAddons }
    | ProductsOnDisc -> { model with DiscProducts = GetProductsOnDisc }
    | Install -> model
    | ApplyOptions -> { model with DlConfig = SetConfig }
    | UpdateDb -> model

let bindings () : Binding<Model, Msg> list =
    [ "DownloadedProducts"
      |> Binding.oneWay (fun m -> m.DownloadedProducts) ]


let GetProductsOnly =
    select {
        for p in productTable do
            selectAll
    }
    |> ProdDbConnectionString.SelectAsync
