module ApDownloader.Core.Project

open System.Net.Http

type FileStatus =
    | IsMissing
    | CanUpdate
    | Ok

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
    { CurrentView: obj
      Products: Product seq
      Client: HttpClient
      DlConfig: DownloadConfig
      ProductManifest: ProductManifest }

type Msg =
    | LoginPage
    | DownloadPage
    | InstallPage
    | OptionsPage
    | Exit
    | Login
    | Logout
    | Download
    | GetDownloaded
    | Install
    | ApplyOptions
    | UpdateDb

let update (msg: Msg) (model: Model) : Model =
    match msg with
    | LoginPage -> failwith "Not implemented"
    | DownloadPage -> failwith "Not implemented"
    | InstallPage -> failwith "Not implemented"
    | OptionsPage -> failwith "Not implemented"
    | Exit -> failwith "Not implemented"
    | Login -> failwith "Not implemented"
    | Logout -> failwith "Not implemented"
    | Download -> failwith "Not implemented"
    | GetDownloaded -> failwith "Not implemented"
    | Install -> failwith "Not implemented"
    | ApplyOptions -> failwith "Not implemented"
    | UpdateDb -> failwith "Not implemented"
