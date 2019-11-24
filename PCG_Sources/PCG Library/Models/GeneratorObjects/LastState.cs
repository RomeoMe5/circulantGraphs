namespace PCG.Library.Models.GeneratorObjects
{
    public class LastState
    {
        public string NodesDescription { get; set; }

        public int[] NotCheckedConfig { get; set; }

        public int[][] GoodConfigs { get; set; }

        public int Dimension { get; set; }

        public float Diameter { get; set; }

        public float AverageDiameter { get; set; }

        public int CurrentNodesCount { get; set; }
    }
}