using UnityEngine;

public abstract class DependenciesContainerBase : MonoBehaviour
{
    #region Fields
    protected bool _alreadyExecuted = false;
    #endregion
    
    #region Methods
    public virtual void InjectDependencies() => Execute<DependenciesContainerBase>();
    
    public virtual void Execute<T>() where T : DependenciesContainerBase
    {
        if(_alreadyExecuted)
            return;

        GameObject parentObject = gameObject;

        Component[] dependenciesInjectionComponents = ComponentsSearcher.GetComponentsOfTypeFromObjectAndAllChildren
            (parentObject, typeof(IDependenciesInjection<T>));

        if (dependenciesInjectionComponents == null && dependenciesInjectionComponents.Length == 0)
            return;
        
        foreach (Component dependenciesInjectionComponent in dependenciesInjectionComponents)
        {
            IDependenciesInjection<T> dependenciesInjection = (IDependenciesInjection<T>)dependenciesInjectionComponent;
            dependenciesInjection.Inject((T)this);
        }

        _alreadyExecuted = true;
    }
    #endregion
}
