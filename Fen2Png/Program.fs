open System.Drawing
open System.Drawing.Imaging
open System.IO
open Fen2Png

[<EntryPoint>]
let main argv = 
    printfn "%A" argv

    let p1 = Pos.FromString("rnbqkbnr/ppp2ppp/4p3/8/2p5/6P1/PP1PPPBP/RNBQK1NR w KQkq - 0 4")
    p1.ToPng("1_1_1.png",true)

    let wrkfol = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "wrk")
    let epds = Directory.GetFiles(wrkfol,"*.epd")|>Array.map(fun e -> new Epd(e))
    epds|>Array.iter(fun e -> e.ToPngs(true))


    0 // return an integer exit code
