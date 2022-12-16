namespace OpenFTTH.RouteNetwork.Business.RouteElements.Model
{
    public record class Envelope
    {
        public double MinX { get; set; }
        public double MinY { get; set; }
        public double MaxX { get; set; }
        public double MaxY { get; set; }


        public Envelope()
        {
            MinX = double.MaxValue;
            MinY = double.MaxValue;
            MaxX = double.MinValue;
            MaxY = double.MinValue;
        }

        public Envelope(double minX, double minY, double maxX, double maxY)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
        }

        public void ExpandToInclude(double x, double y)
        {
            if (MinX > x)
            { 
                MinX = x; 
            }

            if (MinY > y)
            {
                MinY = y;
            }

            if (MaxX < x)
            {
                MaxX = x;
            }

            if (MaxY < y)
            {
                MaxY = y;
            }
        }

        public bool IsWithin(double[] coordArray)
        {
            for (int i = 0; i < coordArray.Length; i+=2)
            {
                if (!IsWithin(coordArray[i], coordArray[i + 1]))
                    return false;
            }

            return true;
        }

        public bool IsWithin(double x, double y) 
        {
            if (x >= MinX && x <= MaxX && y >= MinY && y <= MaxY)
                return true;
            else
                return false;
        }
    }
}
