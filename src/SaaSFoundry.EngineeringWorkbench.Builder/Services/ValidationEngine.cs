namespace SaaSFoundry.EngineeringWorkbench.Builder.Services;


public sealed class ValidationEngine
{

    public bool ValidateArtifact(
        string path)
    {

        return File.Exists(path);

    }

}

