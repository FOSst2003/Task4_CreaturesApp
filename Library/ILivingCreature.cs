// CreaturesLibrary/ILivingCreature.cs
namespace CreaturesLibrary
{
    public interface ILivingCreature
    {
        string Name { get; }
        double Speed { get; }
        void Speak();
        void Move();
        void Stop();
    }
}