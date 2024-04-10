// // psyduck 2024-04-10
namespace PVEDiscovery.Common;

public class QEMUAgentException : Exception
{
    protected QEMUAgentException() {}
    protected QEMUAgentException(string msg) : base(msg) {}
    protected QEMUAgentException(string msg, Exception inner) : base(msg, inner){}
}

public class QEMUAgentNotEnabledException : QEMUAgentException
{
    public QEMUAgentNotEnabledException() {}
    public QEMUAgentNotEnabledException(string msg) : base(msg) {}
    
    public QEMUAgentNotEnabledException(string msg, Exception inner) : base(msg, inner){}
}

public class QEMUAgentNotSupportedException : QEMUAgentException
{
    public QEMUAgentNotSupportedException() {}
    public QEMUAgentNotSupportedException(string msg) : base(msg) {}
    public QEMUAgentNotSupportedException(string msg, Exception inner) : base(msg, inner){}
}

public class QEMUAgentFeatureDisabledException : QEMUAgentException
{
    public QEMUAgentFeatureDisabledException() {}
    public QEMUAgentFeatureDisabledException(string msg) : base(msg) {}
    public QEMUAgentFeatureDisabledException(string msg, Exception inner) : base(msg, inner){}
}

