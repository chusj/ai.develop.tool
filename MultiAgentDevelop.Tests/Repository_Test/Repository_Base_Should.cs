using MultiAgentDevelop.Model.Models;
using Xunit;
using System;
using System.Linq;
using Autofac;
using MultiAgentDevelop.IRepository.Base;

namespace MultiAgentDevelop.Tests
{
    public class Repository_Base_Should
    {
        private const string TestLoginName = "xunit_test_login";

        private IBaseRepository<SysUserInfo> baseRepository;
        DI_Test dI_Test = new DI_Test();

        public Repository_Base_Should()
        {

            var container = dI_Test.DICollections();

            baseRepository = container.Resolve<IBaseRepository<SysUserInfo>>();

            //DbContext.Init(BaseDBConfig.ConnectionString,(DbType)BaseDBConfig.DbType);
        }


        [Fact]
        public async void Get_Users_Test()
        {
            var data = await baseRepository.Query();

            Assert.NotNull(data);
        }

        [Fact]
        public async void Add_User_Test()
        {
            SysUserInfo user = new SysUserInfo()
            {
                LoginName = TestLoginName,
                Name = "xunit test",
                RealName = "xunit test realname",
                Status = 0,
                Remark = "xunit： test repositoryBase add user",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                LastErrorTime = DateTime.Now,
            };

            var id = await baseRepository.Add(user);
            Assert.True(id > 0);
        }


        [Fact]
        public async void Update_User_Test()
        {
            var IsUpd = false;
            var updateModel = (await baseRepository.Query(d => d.LoginName == TestLoginName)).FirstOrDefault();

            Assert.NotNull(updateModel);

            updateModel.RealName = "xunit: test repositoryBase realname update";
            updateModel.UpdateTime = DateTime.Now;

            IsUpd = await baseRepository.Update(updateModel);

            Assert.True(IsUpd);
        }

        [Fact]
        public async void Delete_User_Test()
        {
            var IsDel = false;
            var deleteModel = (await baseRepository.Query(d => d.LoginName == TestLoginName)).FirstOrDefault();

            Assert.NotNull(deleteModel);

            IsDel = await baseRepository.Delete(deleteModel);

            Assert.True(IsDel);
        }
    }
}
