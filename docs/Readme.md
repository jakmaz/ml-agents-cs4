# ML-Agents Project - Computer Science Group 4

This project is based on the code from the `release_21` version of the official [ML-Agents repository](https://github.com/Unity-Technologies/ml-agents). Due to the complexity and messiness with the branches in the official repository, we decided to start fresh by downloading and working off the release code.

## Project Structure

The original ML-Agents repository includes a Unity project containing multiple example environments. We use this as a reference while developing our own separate project, named **SoccerProject**. All project-specific code and features are located in **SoccerProject**, providing a clean and dedicated space for further development.

Our initial step was to extend the original soccer game example by adding a new rule that penalizes players for colliding with opponents. This change encourages agents to avoid fouls. After implementing this feature, we trained a new model, which made the agents significantly more careful during gameplay.

![image](https://github.com/user-attachments/assets/aea8dca9-89f4-4346-88a9-a773a76671e5)


## Getting Started

### 1. Clone the Repository
Open a terminal and navigate to the directory where you'd like to clone the project code. Run the following command to clone the repository:

```bash
git clone https://github.com/jakmaz/ml-agents-cs4
```

### 2. Open the Project in Unity Hub
Once you have cloned the repository, follow these steps to open the project in Unity Hub:

1. Open Unity Hub and click the **Add** button.
![SCR-20240911-mmlo](https://github.com/user-attachments/assets/5c790056-cc8a-4edc-bd2a-dddfc599e6b3)
2. Navigate to the cloned repository project and import SoccerProject into Unity Hub.
![image](https://github.com/user-attachments/assets/9bdcfc2a-dba8-4668-890a-2f2dcd672171)
3. Once the project is imported, you can open it in Unity. When opening the project, Unity may prompt you to download the correct version of the Unity Editor. Follow the instructions to download and install the required editor version.
![SCR-20240911-tdlm](https://github.com/user-attachments/assets/828ae34b-27f8-44e4-b6dd-46401a9347e8)

### 3. Download the Correct Editor Version
When opening the project, Unity may prompt you to download the correct version of the Unity Editor. Follow the instructions to download and install the required editor version.

### 4. Running the App
After setting up the correct editor version, you should be able to run the app directly from Unity Hub.

### 5. Getting Started with ML-Agents
For setting a python environment as well as a basic introduction to working with ML-Agents, follow this [Getting Started guide](https://github.com/Unity-Technologies/ml-agents/blob/develop/docs/Getting-Started.md) from the official documentation.

### 6. Full Documentation
For a more comprehensive understanding of ML-Agents, visit the [official documentation](https://github.com/Unity-Technologies/ml-agents/tree/develop) for detailed instructions and resources.
