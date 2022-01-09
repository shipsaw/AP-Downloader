module ApDownloader.Core.Project

open System.Net.Http

type FileStatus =
    | IsMissing
    | CanUpdate
    | Ok

type CurrentPage =
    | LoginPage
    | DownloadPage
    | InstallPage
    | OptionsPage

type DownloadConfig =
    { GetExtraStock: bool
      GetBrandingPatch: bool
      GetLiveryPack: bool
      DownloadFilepath: string
      InstallFilepath: string }

type Product =
    { ProductId: int
      Name: string
      Filename: string
      ImageName: string
      FileStatus: FileStatus
      ServerContentLength: int64
      CurrentContentLength: int64 }

type ProductManifest =
    { ProductIds: string seq
      ProductFilenames: string seq
      ExtraStockFilenames: string seq
      BrandingPatchesFilenames: string seq
      LiveryPackFilenames: string seq }

type Model =
    { CurrentPage: CurrentPage
      AllApProducts: Product seq
      PurchasedProducts: Product seq
      DownloadedProducts: Product seq
      DiscProducts: Product seq
      Client: HttpClient option
      DlConfig: DownloadConfig
      ProductManifest: ProductManifest }

let init () =
    { CurrentPage = LoginPage
      AllApProducts = Seq.empty
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
