using NUnit.Framework;

namespace PVMRM
{
    //[TestFixture]
    public class UtHardCodedVars
    {
        //[Test]
        public void CheckHardCodedPeptideList()
        {
            HardCodedVars hcv = new HardCodedVars();
            hcv.SetHardCodedVars();
            Assert.True(hcv.HardCodedPeptideList.Count>0);
        }
    }
}
