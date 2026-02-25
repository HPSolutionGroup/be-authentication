namespace be_authenticationDomain.Entities
{
    public class CommandInFunction
    {
        public Guid FunctionId { get; set; }
        public Function Function { get; set; }

        public Guid CommandId { get; set; }
        public Command Command { get; set; }
    }
}
