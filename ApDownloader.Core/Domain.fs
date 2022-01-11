namespace Domain

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
