namespace Fen2Png

open System
open System.Text
open System.Drawing
open System.Drawing.Imaging
open System.IO


type Pos(isqs : char [], iisw : bool) = 
    let sqs = isqs
    let mutable isw = iisw
    
    member val Sqs = sqs with get, set
    member val IsW = isw with get, set
    member val CustomizationFunctions = [] with get, set
    
    //private member to parse string
    static member private Parse(s : string) = 
        let b = s.Split(' ')
        let isw = b.[1] = "w"
        let sqs = Array.create 64 ' '
        
        let rec getp i ps = 
            if ps = "" then sqs, isw
            else 
                match ps.[0] with
                | '/' -> getp i ps.[1..]
                | c -> 
                    let ok, p = Int32.TryParse(c.ToString())
                    if ok then getp (i + p) ps.[1..]
                    else 
                        sqs.[i] <- c
                        getp (i + 1) ps.[1..]
        getp 0 b.[0]
    
    /// loads Pos given a FEN like string
    static member FromString(s : string) = 
        let isqs, iisw = Pos.Parse(s)
        new Pos(isqs, iisw)
    
    /// Gets initial Pos
    static member Start() = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w" |> Pos.FromString
    
    member x.Set(s : string) = 
        let sqs, isw = Pos.Parse(s)
        sqs |> Array.iteri (fun i c -> x.Sqs.[i] <- c)
        x.IsW <- isw
    
    override x.ToString() = 
        let sb = new StringBuilder()
        
        let rec getstr i e chl = 
            if List.isEmpty chl then 
                if e > 0 then sb.Append(e.ToString()) |> ignore
                sb.ToString() + (if x.IsW then " w"
                                 else " b")
            elif i <> 8 && chl.Head = ' ' then getstr (i + 1) (e + 1) chl.Tail
            else 
                if e > 0 then sb.Append(e.ToString()) |> ignore
                if i = 8 then sb.Append("/") |> ignore
                else sb.Append(chl.Head) |> ignore
                if i = 8 then getstr 0 0 chl
                else getstr (i + 1) 0 chl.Tail
        getstr 0 0 (x.Sqs |> List.ofArray)
    
    member x.Copy() = new Pos((x.Sqs |> Array.copy), x.IsW)

    member x.ToPng(pngName:string, flip:bool) = 
        let imgsfol = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "imgs")
        let c2tl i c =
            let x = i%8
            let y = i/8
            let sqcol = if (x+y)%2=0 then "l" else "d"
            let pccol = if c.ToString().ToUpper() = c.ToString() then "l" else "d"
            let pc = c.ToString()
            let fn = if c=' '  then "Chess_" + sqcol + "44.png" else "Chess_" + pc + pccol + sqcol + "44.png"
            new Bitmap(Path.Combine(imgsfol,fn))
        let tls = x.Sqs|>Array.mapi c2tl
    
        let w = 44
        let h = 44
        let size = new Size(w,h)

        let image =  new Bitmap(8*w,8*h,PixelFormat.Format32bppArgb)
        for y = 0 to 7 do
            for x = 0 to 7 do
                let location = if flip then new Point((7-x)*w,(7-y)*h) else new Point(x*w,y*h)
                let tl = tls.[y*8+x]
                Graphics.FromImage(image).DrawImage(tl,new Rectangle(location,size))
        image.Save(pngName, ImageFormat.Png)
        
type Epd(ifn : string) = 
    let fn = ifn
    let fens = File.ReadAllLines(fn)
    let poss = fens|>Array.map(Pos.FromString)
    member val Fens = fens with get, set
    member val Poss = poss with get, set
    member x.Prefix = Path.GetFileNameWithoutExtension(fn).Replace(".","_")

    member x.ToPngs(flip:bool) =
        let toPng i (p:Pos) = 
            let fol = Path.GetDirectoryName(fn)
            let pn = Path.Combine(fol, x.Prefix + "_" + (i+1).ToString() + ".png")
            p.ToPng(pn,flip)
        x.Poss|>Array.iteri(toPng)