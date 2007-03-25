using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Globalization;
using MediaPortal.Localisation.LanguageStrings;

namespace MediaPortal.Localisation
{
  public class LocalisationProvider : ILocalisation
  {
    #region Variables
    Dictionary<string, Dictionary<int, StringLocalised>> _languageStrings;
    Dictionary<string, CultureInfo> _availableLanguages;
    List<string> _languageDirectories;
    CultureInfo _currentLanguage;
    string _systemDirectory;
    string _userDirectory;
    int _characters;
    bool _prefix;
    bool _userLanguage;
    #endregion

    #region Constructors/Destructors
    public LocalisationProvider(string systemDirectory, string userDirectory, string cultureName, bool prefix)
    {
      // Base strings directory
      _systemDirectory = systemDirectory;
      // User strings directory
      _userDirectory = userDirectory;

      _prefix = prefix;

      _languageDirectories = new List<string>();
      _languageDirectories.Add(_systemDirectory);

      GetAvailableLangauges();

      // If the language cannot be found default to Local language or English
      if (cultureName != null && _availableLanguages.ContainsKey(cultureName))
        _currentLanguage = _availableLanguages[cultureName];
      else
        _currentLanguage = GetBestLanguage();

      if (_currentLanguage == null)
        throw (new ArgumentException("No available language found"));

      _languageStrings = new Dictionary<string, Dictionary<int, StringLocalised>>();

      CheckUserStrings();
      ReloadAll();
    }

    public LocalisationProvider(string directory, string cultureName, bool prefix)
      : this(directory, directory, cultureName, prefix)
    {
    }

    public LocalisationProvider(string directory, string cultureName)
      : this(directory, cultureName, true)
    {
    }

    public void Dispose()
    {
      Clear();
    }
    #endregion

    #region Properties
    public CultureInfo CurrentLanguage
    {
      get { return _currentLanguage; }
    }

    public int Characters
    {
      get { return _characters; }
    }
    #endregion

    #region Public Methods
    public void AddDirection(string directory)
    {
      // Add directory to list, to enable reloading/changing language
      _languageDirectories.Add(directory);

      LoadStrings(directory);
    }

    public void ChangeLanguage(string cultureName)
    {
      if (!_availableLanguages.ContainsKey(cultureName))
        throw new ArgumentException("Language not available");

      _currentLanguage = _availableLanguages[cultureName];

      ReloadAll();
    }

    public string Get(string section, int id)
    {
      if (_languageStrings.ContainsKey(section.ToLower()) && _languageStrings[section].ContainsKey(id))
      {
        string prefix = string.Empty;
        if (_prefix)
          prefix = _languageStrings[section.ToLower()][id].prefix;

        return prefix + _languageStrings[section.ToLower()][id].text;
      }

      return null;
    }

    public string Get(string section, int id, object[] parameters)
    {
      string translation = Get(section, id);
      // if parameters or the translation is null, return the translation.
      if ((translation == null) || (parameters == null))
      {
        return translation;
      }
      // return the formatted string. If formatting fails, log the error
      // and return the unformatted string.
      try
      {
        return String.Format(translation, parameters);
      }
      catch (System.FormatException)
      {
        //Log.Error("Error formatting translation with id {0}", dwCode);
        //Log.Error("Unformatted translation: {0}", translation);
        //Log.Error(e);  
        // Throw exception??
        return translation;
      }
    }

    public CultureInfo[] AvailableLanguages()
    {
      CultureInfo[] available = new CultureInfo[_availableLanguages.Count];

      IDictionaryEnumerator languageEnumerator = _availableLanguages.GetEnumerator();

      for (int i = 0; i < _availableLanguages.Count; i++)
      {
        languageEnumerator.MoveNext();
        available[i] = (CultureInfo)languageEnumerator.Value;
      }

      return available;
    }

    public bool IsLocalSupported()
    {
      if (_availableLanguages.ContainsKey(CultureInfo.CurrentCulture.Name))
        return true;

      return false;
    }

    public CultureInfo GetBestLanguage()
    {
      // Try current local language
      if (_availableLanguages.ContainsKey(CultureInfo.CurrentCulture.Name))
        return CultureInfo.CurrentCulture;

      // Try Language Parent if it has one
      if (!CultureInfo.CurrentCulture.IsNeutralCulture &&
        _availableLanguages.ContainsKey(CultureInfo.CurrentCulture.Parent.Name))
        return CultureInfo.CurrentCulture.Parent;

      // default to English
      if (_availableLanguages.ContainsKey("en"))
        return _availableLanguages["en"];

      return null;
    }
    #endregion

    #region Private Methods
    private void LoadUserStrings()
    {
      // Load User Custom strings
      if (_userLanguage)
        GetStrings(_userDirectory, "strings_user.xml");
    }

    private void LoadStrings(string directory)
    {
      // Local Language
      GetStrings(directory, "strings_" + _currentLanguage.Name + ".xml");

      // Parent Language
      if (!_currentLanguage.IsNeutralCulture)
        GetStrings(directory, "strings_" + _currentLanguage.Parent.Name + ".xml");

      // Default to English
      GetStrings(directory, "strings_en.xml");
    }

    private void ReloadAll()
    {
      Clear();

      LoadUserStrings();

      foreach (string directoy in _languageDirectories)
        LoadStrings(directoy);
    }

    private void Clear()
    {
      if (_languageStrings != null)
        _languageStrings.Clear();

      _characters = 255;
    }

    private void CheckUserStrings()
    {
      _userLanguage = false;

      string path = Path.Combine(_userDirectory, "strings_user.xml");

      if (File.Exists(path))
        _userLanguage = true;
    }

    private void GetAvailableLangauges()
    {
      _availableLanguages = new Dictionary<string, CultureInfo>();

      DirectoryInfo dir = new DirectoryInfo(_systemDirectory);
      foreach (FileInfo file in dir.GetFiles("strings_*.xml"))
      {
        int pos = file.Name.IndexOf('_') + 1;
        string cultName = file.Name.Substring(pos, file.Name.Length - file.Extension.Length - pos);

        try
        {
          CultureInfo cultInfo = new CultureInfo(cultName);
          _availableLanguages.Add(cultName, cultInfo);
        }
        catch (ArgumentException)
        {
          // Log file error?
        }

      }
    }

    private void GetStrings(string directory, string filename)
    {
      string path = Path.Combine(directory, filename);
      if (File.Exists(path))
      {
        StringFile strings;
        try
        {
          XmlSerializer s = new XmlSerializer(typeof(StringFile));
          TextReader r = new StreamReader(path);
          strings = (StringFile)s.Deserialize(r);
        }
        catch (Exception)
        {
          return;
        }

        if (_characters < strings.characters)
          _characters = strings.characters;

        foreach (StringSection section in strings.sections)
        {
          // convert section name tolower -> no case matching.
          section.name = section.name.ToLower();

          Dictionary<int, StringLocalised> newSection;
          if (_languageStrings.ContainsKey(section.name))
          {
            newSection = _languageStrings[section.name];
            _languageStrings.Remove(section.name);
          }
          else
          {
            newSection = new Dictionary<int, StringLocalised>();
          }

          foreach (StringLocalised languageString in section.localisedStrings)
          {
            if (!newSection.ContainsKey(languageString.id))
              newSection.Add(languageString.id, languageString);
          }

          if (newSection.Count > 0)
            _languageStrings.Add(section.name, newSection);
        }
      }
    }
    #endregion
  }
}
