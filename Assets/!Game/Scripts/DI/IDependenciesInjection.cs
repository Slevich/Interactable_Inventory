using UnityEngine;

public interface IDependenciesInjection<T>
where T : DependenciesContainerBase
{
    public bool Initialized { get; set; }
    
    public void Inject(T container) {}
    public void Initialize() {}
}