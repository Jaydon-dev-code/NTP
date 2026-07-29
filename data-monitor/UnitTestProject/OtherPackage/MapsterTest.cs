using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapster;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory4Workshop4_10;
using SL.MLineDataPrecisionTracking.Models.Dtos.Factory4Workshop4_10Line;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory4Workshop4_10Line;

namespace UnitTestProject.OtherPackage
{
    [TestClass]
    public class MapsterTest
    {
        [TestMethod]
        public void MapTest()
        {
            Tb_Factory4Workshop4_10Line_Clearance soure =
                new Tb_Factory4Workshop4_10Line_Clearance()
                {
                    AfterPressIn = "sfdaa",
                    BeforePressIn = "222",
                    SN = "asddasdadsa",
                    Gap = "23.4",
                };
            var val = soure.Adapt<Factory4Workshop4_10Line_ClearanceDto>();
            Assert.AreEqual(soure.AfterPressIn, val.AfterPressIn);
            Assert.AreEqual(soure.BeforePressIn, val.BeforePressIn);
            Assert.AreEqual(soure.SN, val.SN);
            Assert.AreEqual(soure.Gap, val.Gap);
            Assert.AreEqual(soure.RecordTime, val.RecordTime);

        }
    }
}
