# Oeeng.IntegrationTesting

Oeeng.IntegrationTesting is a C# project designed to facilitate integration testing for user interface automation. It provides tools to record and replay user interactions with applications, enabling automated testing scenarios.

## Features

- **UI Automation Recorder**: Capture user interactions with applications to create reproducible test scripts.
- **Playback Functionality**: Replay recorded actions to validate application behavior.
- **Integration with FlaUI**: Utilize the FlaUI library for robust UI automation across various application types.

## Prerequisites

- .NET Framework 4.7.2 or higher
- Visual Studio 2019 or later
- FlaUI library

## Getting Started

1. **Clone the Repository**:

   ```bash
   git clone https://github.com/OezdemirEngineering/Oeeng.IntegrationTesting.git
   ```

2. **Open the Solution**:

   Open `Tests.sln` in Visual Studio.

3. **Restore NuGet Packages**:

   Ensure all necessary NuGet packages are restored.

4. **Build the Solution**:

   Build the solution to resolve dependencies and compile the projects.

5. **Run the Application**:

   Set `UiAutomationRecorder` as the startup project and run the application.

## Usage

- **Recording Actions**:

  1. Launch the `UiAutomationRecorder` application.
  2. Select the target application from the list of running processes.
  3. Click "Attach" to connect to the selected application.
  4. Click "Start Recording" to begin capturing user interactions.
  5. Perform the desired actions in the target application.
  6. Click "Stop Recording" to end the session.
  7. Save the recorded actions to a JSON file when prompted.

- **Loading and Running Scripts**:

  1. Click "Load Actions" to import a previously saved JSON script.
  2. Review the loaded actions in the list view.
  3. Click "Run" to execute the loaded actions on the attached application.
  4. Use "Stop Execution" to halt the playback if necessary.

## Project Structure

- **Accessor.Base**: Contains base classes and methods for interacting with UI elements.
- **Login**: Includes functionality related to user authentication scenarios.
- **Tests**: Houses test cases and related resources.
- **UiAutomationRecorder**: The main application for recording and replaying user interactions.

## Contributing

Contributions are welcome! Please fork the repository and create a pull request with your enhancements or bug fixes.

## License

This project is licensed under the MIT License. See the `LICENSE` file for more details.

## Acknowledgments

Special thanks to the developers of [FlaUI](https://github.com/FlaUI/FlaUI) for providing a comprehensive UI automation library.

---

For more information, visit the [Oeeng.IntegrationTesting GitHub repository](https://github.com/OezdemirEngineering/Oeeng.IntegrationTesting). 
