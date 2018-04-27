open System
open System.IO
open Fen2Png

[<EntryPoint>]
let main argv = 
    if argv.Length<>2 then
        Console.WriteLine "Need to provide 2 arguments: 1. Folder containing epd file(s) 2. b/w specifying needing diagram from Black's or White's view."
        1
    else
        let ifol = argv.[0]
        let flip = argv.[1]="b"
        let fol = if ifol.[1]=':' then ifol else Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), ifol)
        if not (Directory.Exists fol) then
            Console.WriteLine("Invalid folder supplied: " + fol)
            1
        else
            Console.WriteLine("Using folder: " + fol)
            if flip then Console.WriteLine("Diagrams are from Black's view") else Console.WriteLine("Diagrams are from White's view")
            let epds = Directory.GetFiles(fol,"*.epd")|>Array.map(fun e -> new Epd(e))
            if epds.Length=0 then 
                Console.WriteLine("No EPD files found in folder: " + fol)
                1
            else
                epds|>Array.iter(fun e -> e.ToPngs(flip))
                Console.WriteLine("Finished creating PNG diagrams in folder: " + fol)

                0 // return an integer exit code
