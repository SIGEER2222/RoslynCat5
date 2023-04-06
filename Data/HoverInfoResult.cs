//using RoslynCat.Interface;

//namespace RoslynCat.Data
//{
//    public class HoverInfoResult : IResponse
//    {
//        public HoverInfo hoverInfo { get; set; }

//        public HoverInfoResult(HoverInfo hoverInfo) {
//            this.hoverInfo = hoverInfo;
//        }

//        public HoverInfoResult(string? information,int offsetFrom,int offsetTo) {
//            Information = information;
//            OffsetFrom = offsetFrom;
//            OffsetTo = offsetTo;
//        }

//        public HoverInfoResult()
//        {
//        }

//        public virtual string? Information { get; set; }

//        public virtual int OffsetFrom { get; set; }

//        public virtual int OffsetTo { get; set; }
//    }
//    public class HoverInfo {
//        public string? Type { get; set; }
//        public string? Context { get; set; }
//        public string? Parameter { get; set; }
//        public string? Exegesis { get; set; }
//        public string? Other { get; set; }
//    }
//}
