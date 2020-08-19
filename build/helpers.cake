public class ProjectCollection {
    public IEnumerable<SolutionProject> SourceProjects {get;set;}
    public IEnumerable<DirectoryPath> SourceProjectPaths {get { return SourceProjects.Select(p => p.Path.GetDirectory()); } } 
    public IEnumerable<SolutionProject> TestProjects {get;set;}
    public IEnumerable<DirectoryPath> TestProjectPaths { get { return TestProjects.Select(p => p.Path.GetDirectory()); } }
    public IEnumerable<SolutionProject> AllProjects { get { return SourceProjects.Concat(TestProjects); } }
    public IEnumerable<DirectoryPath> AllProjectPaths { get { return AllProjects.Select(p => p.Path.GetDirectory()); } }
}

ProjectCollection GetProjects(FilePath slnPath, string configuration) {
    var solution = ParseSolution(slnPath);
    var projects = solution.Projects.Where(p => p.Type != "{2150E333-8FDC-42A3-9474-1A3956D46DE8}");
    var testAssemblies = projects.Where(p => p.Name.Contains(".Tests")).Select(p => p.Path.GetDirectory() + "/bin/" + configuration + "/" + p.Name + ".dll");
    return new ProjectCollection {
        SourceProjects = projects.Where(p => !p.Name.Contains(".Tests")),
        TestProjects = projects.Where(p => p.Name.Contains(".Tests"))
    };
    
}

public Dictionary<string, string> GetPackageFormats() {
    return new Dictionary<string, string> {
        ["fc32"] = "-t rpm -d libunwind -d libicu",
        ["el8"] = "-t rpm -d libicu",
        ["bionic"] = "-t deb -d libicu60 -d libssl1.0.0"
    };
}
