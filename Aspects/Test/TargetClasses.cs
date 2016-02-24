
using System;
namespace vm.Aspects.Tests
{
    #region Test target classes and interfaces
    public interface ITestTarget
    {
        string IdentifySource();
        string IdentifyUniqueSource();
    }

    public class RegisteredTargetTypeInCode
    {
        public string IdentifySource() => "type in code";

        Guid uniqueId = Guid.NewGuid();

        public string IdentifyUniqueSource() => string.Format("{0} ({1})", IdentifySource(), uniqueId);
    }

    public class RegisteredTargetTypeInAppConfig
    {
        public string IdentifySource() => "type in app.config";

        Guid uniqueId = Guid.NewGuid();

        public string IdentifyUniqueSource() => string.Format("{0} ({1})", IdentifySource(), uniqueId);
    }

    public class RegisteredTargetTypeInTestConfig
    {
        public string IdentifySource() => "type in test.config";

        Guid uniqueId = Guid.NewGuid();

        public string IdentifyUniqueSource() => string.Format("{0} ({1})", IdentifySource(), uniqueId);
    }

    /// <summary>
    /// Class DIContainerFromAppConfig. The registration is in code.
    /// </summary>
    public class TestTargetFromCode : ITestTarget
    {
        public string IdentifySource() => "from code";

        Guid uniqueId = Guid.NewGuid();

        public string IdentifyUniqueSource() => string.Format("{0} ({1})", IdentifySource(), uniqueId);
    }

    /// <summary>
    /// Class DIContainerFromAppConfig. The registration is in code.
    /// </summary>
    public class TestTargetFromCodeBox : ITestTarget
    {
        public string IdentifySource() => "from code/box";

        Guid uniqueId = Guid.NewGuid();

        public string IdentifyUniqueSource() => string.Format("{0} ({1})", IdentifySource(), uniqueId);
    }

    /// <summary>
    /// Class DIContainerFromAppConfig. The registration is in app.config.
    /// </summary>
    public class TestTargetFromAppConfig : ITestTarget
    {
        public string IdentifySource() => "from app.config";

        Guid uniqueId = Guid.NewGuid();

        public string IdentifyUniqueSource() => string.Format("{0} ({1})", IdentifySource(), uniqueId);
    }

    /// <summary>
    /// Class DIContainerFromAppConfig. The registration is in app.config.
    /// </summary>
    public class TestTargetFromAppConfigBox : ITestTarget
    {
        public string IdentifySource() => "from app.config/box";

        Guid uniqueId = Guid.NewGuid();

        public string IdentifyUniqueSource() => string.Format("{0} ({1})", IdentifySource(), uniqueId);
    }

    /// <summary>
    /// Class DIContainerFromAppConfig. The registration is in test.config.
    /// </summary>
    public class TestTargetFromTestConfig : ITestTarget
    {
        public string IdentifySource() => "from test.config";

        Guid uniqueId = Guid.NewGuid();

        public string IdentifyUniqueSource() => string.Format("{0} ({1})", IdentifySource(), uniqueId);
    }

    /// <summary>
    /// Class DIContainerFromAppConfig. The registration is in test.config.
    /// </summary>
    public class TestTargetFromTestConfigBox : ITestTarget
    {
        public string IdentifySource() => "from test.config/box";

        Guid uniqueId = Guid.NewGuid();

        public string IdentifyUniqueSource() => string.Format("{0} ({1})", IdentifySource(), uniqueId);
    }

    #endregion
}
