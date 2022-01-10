module ApDownloader.Core.DataAccess

open Microsoft.Data.Sqlite
open FSharp.Data.Dapper

let private mkOnDiskConnectionString (dataSource: string) = sprintf "Data Source = %s;" dataSource

let mkOnDisk () =
    new SqliteConnection(mkOnDiskConnectionString "./ProductsDb.db")

let connectionF () = SqliteConnection(mkOnDisk ())
let querySeqAsync<'R> = querySeqAsync<'R> (connectionF)
let querySingleAsync<'R> = querySingleOptionAsync<'R> (connectionF)

[<CLIMutable>]
type Product =
    { ProductId: int
      Name: string
      Filename: string
      ImageName: string }

let GetAll =
    querySeqAsync<Product> { script "SELECT * FROM Product" }

let GetAll () =
    querySeqAsync<Product> { script "SELECT * FROM Bird" }

printfn "== GetAll"

GetAll
|> Async.RunSynchronously
|> Seq.iter (printfn "%A")
