import { IMcpToolDefinition } from "../core/interfaces/ICommandHandler.js";
import { JObject } from "../types/index.js";
import { z } from "zod";
import { BaseCommandHandler } from "../core/BaseCommandHandler.js";

/**
 * MCP tools for creating and modifying GameObjects in the active Unity scene.
 * Forwards to Unity C# SceneCommandHandler (scene.*).
 */
export class SceneCommandHandler extends BaseCommandHandler {
  public get commandPrefix(): string {
    return "scene";
  }

  public get description(): string {
    return "Create and modify GameObjects in the active scene";
  }

  public getToolDefinitions(): Map<string, IMcpToolDefinition> {
    const tools = new Map<string, IMcpToolDefinition>();

    tools.set("scene_CreateGameObject", {
      description: "Create an empty GameObject in the active scene",
      parameterSchema: {
        name: z.string().optional().describe("Object name (default: GameObject)"),
        parentPath: z.string().optional().describe("Parent path e.g. Canvas/Panel")
      },
      annotations: {
        title: "Create GameObject",
        readOnlyHint: false,
        destructiveHint: false,
        idempotentHint: false,
        openWorldHint: false
      }
    });

    tools.set("scene_AddComponent", {
      description: "Add a component to a GameObject by path",
      parameterSchema: {
        path: z.string().describe("GameObject path e.g. CitizenManager"),
        componentType: z.string().describe("Component class name e.g. CitizenManager")
      },
      annotations: {
        title: "Add Component",
        readOnlyHint: false,
        destructiveHint: false,
        idempotentHint: false,
        openWorldHint: false
      }
    });

    tools.set("scene_SetPosition", {
      description: "Set GameObject position",
      parameterSchema: {
        path: z.string().describe("GameObject path"),
        x: z.number().optional().describe("X"),
        y: z.number().optional().describe("Y"),
        z: z.number().optional().describe("Z")
      },
      annotations: {
        title: "Set Position",
        readOnlyHint: false,
        destructiveHint: false,
        idempotentHint: false,
        openWorldHint: false
      }
    });

    tools.set("scene_SetParent", {
      description: "Set parent of a GameObject",
      parameterSchema: {
        path: z.string().describe("GameObject path"),
        parentPath: z.string().optional().describe("New parent path, empty to unparent")
      },
      annotations: {
        title: "Set Parent",
        readOnlyHint: false,
        destructiveHint: false,
        idempotentHint: false,
        openWorldHint: false
      }
    });

    tools.set("scene_Find", {
      description: "Find a GameObject by path",
      parameterSchema: {
        path: z.string().describe("GameObject path e.g. Canvas/Button")
      },
      annotations: {
        title: "Find GameObject",
        readOnlyHint: true,
        destructiveHint: false,
        idempotentHint: true,
        openWorldHint: false
      }
    });

    tools.set("scene_SaveScene", {
      description: "Save the currently active scene to disk",
      parameterSchema: {},
      annotations: {
        title: "Save Scene",
        readOnlyHint: false,
        destructiveHint: false,
        idempotentHint: false,
        openWorldHint: false
      }
    });

    tools.set("scene_OpenScene", {
      description: "Open a scene by path (e.g. Assets/Scenes/BaseScene.unity)",
      parameterSchema: {
        path: z.string().describe("Scene path relative to project e.g. Assets/Scenes/BaseScene.unity")
      },
      annotations: {
        title: "Open Scene",
        readOnlyHint: false,
        destructiveHint: true,
        idempotentHint: false,
        openWorldHint: false
      }
    });

    tools.set("scene_GetActiveSceneName", {
      description: "Get the name and path of the currently active scene",
      parameterSchema: {},
      annotations: {
        title: "Get Active Scene Name",
        readOnlyHint: true,
        destructiveHint: false,
        idempotentHint: true,
        openWorldHint: false
      }
    });

    tools.set("scene_InstantiatePrefab", {
      description: "Instantiate a prefab from Assets into the active scene (optionally under a parent)",
      parameterSchema: {
        prefabPath: z.string().describe("Asset path e.g. Assets/Prefabs/MyPrefab.prefab"),
        parentPath: z.string().optional().describe("Parent hierarchy path e.g. Canvas/Panel")
      },
      annotations: {
        title: "Instantiate Prefab",
        readOnlyHint: false,
        destructiveHint: false,
        idempotentHint: false,
        openWorldHint: false
      }
    });

    tools.set("scene_ListRootObjects", {
      description: "List root GameObject names in the active scene hierarchy",
      parameterSchema: {},
      annotations: {
        title: "List Root Objects",
        readOnlyHint: true,
        destructiveHint: false,
        idempotentHint: true,
        openWorldHint: false
      }
    });

    return tools;
  }

  protected async executeCommand(action: string, parameters: JObject): Promise<JObject> {
    // action = "CreateGameObject" | "AddComponent" | "SetPosition" | "SetParent" | "Find"
    // (HandlerAdapter extracts action from tool name: scene_CreateGameObject -> CreateGameObject)
    return await this.sendUnityRequest(`${this.commandPrefix}.${action}`, parameters);
  }
}
