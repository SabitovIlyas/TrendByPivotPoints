namespace TradingSystems.Tests
{
    public class ClassForTestKeyWords
    {
        public void MethodWithKeyWordRefForStruct(ref int param)
        {
            param++;
        }

        public void MethodWithoutKeyWordRefForStruct(int param)
        {
            param++;
        }

        public void MethodWithKeyWordRefForClass(ref HelperForClassForTestKeyWords param)
        {
            param = new HelperForClassForTestKeyWords(2);
        }

        public void MethodWithoutKeyWordRefForClass(HelperForClassForTestKeyWords param)
        {
            param = new HelperForClassForTestKeyWords(2);
        }

        public void MethodWithKeyWordOutForClass(out int param)
        {
            param = 1;
        }

        public void MethodWithoutKeyWordOutForClass(int param)
        {
            param++;
        }
    }
}