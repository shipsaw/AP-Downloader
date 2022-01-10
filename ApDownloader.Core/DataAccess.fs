module ApDownloader.Core.DataAccess

open Microsoft.Data.Sqlite
open FSharp.Data.Dapper

let private mkOnDiskConnectionString (dataSource: string) =
    $"Data Source = %s{dataSource};"
let mkOnDisk () = new SqliteConnection (mkOnDiskConnectionString "./ProductsDb.db")
let connectionF () = SqliteConnection (mkOnDisk())
let querySeqAsync<'R> = querySeqAsync<'R> (connectionF)
let querySingleAsync<'R> = querySingleOptionAsync<'R> (connectionF)
 
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
      
let GetAllBaseProducts = querySeqAsync<DbProduct> {
    script "SELECT * FROM Product"
    //parameters (dict ["Name", box name])
}

let ToFunctionalOption (dbProduct:DbProduct) : Product =
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
    let GetAllProducts = querySeqAsync<DbProduct> {
        script "SELECT Name, Product.Filename, ImageName,
        ES.Filename AS EsFilename,
        BP.Filename AS BpFilename,
        LP.Filename AS LpFilename FROM Product
        LEFT JOIN ExtraStock AS ES ON ES.ProductID = Product.ProductID
        LEFT JOIN BrandingPatch AS BP ON BP.ProductID = Product.ProductID
        LEFT JOIN LiveryPack AS LP ON LP.ProductID = Product.ProductID" }
    // Remove the nulls
    let OptionProducts = GetAllProducts
                             |> Async.RunSynchronously
                             |> Seq.cast<DbProduct>
                             |> Seq.map (fun dbP -> ToFunctionalOption dbP)
    OptionProducts