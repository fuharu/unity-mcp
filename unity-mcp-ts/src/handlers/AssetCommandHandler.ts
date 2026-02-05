import { IMcpToolDefinition } from "../core/interfaces/ICommandHandler.js";
import { JObject } from "../types/index.js";
import { z } from "zod";
import { BaseCommandHandler } from "../core/BaseCommandHandler.js";

/**
 * MCP tools for searching assets in the Unity project.
 * Forwards to Unity C# AssetCommandHandler (asset.*).
 */
export class AssetCommandHandler extends BaseCommandHandler {
  public get commandPrefix(): string {
    return "asset";
  }

  public get description(): string {
    return "Search assets in the project by name or type";
  }

  public getToolDefinitions(): Map<string, IMcpToolDefinition> {
    const tools = new Map<string, IMcpToolDefinition>();

    tools.set("asset_Search", {
      description: "Search project assets by query. Optionally filter by type (e.g. Prefab, Scene, Script).",
      parameterSchema: {
        query: z.string().describe("Search text (name or keyword)"),
        type: z.string().optional().describe("Filter by type: Prefab, Scene, Script, Texture2D, etc."),
        limit: z.number().optional().describe("Max results (1-200, default 50)")
      },
      annotations: {
        title: "Search Assets",
        readOnlyHint: true,
        destructiveHint: false,
        idempotentHint: true,
        openWorldHint: false
      }
    });

    return tools;
  }

  protected async executeCommand(action: string, parameters: JObject): Promise<JObject> {
    return await this.sendUnityRequest(`${this.commandPrefix}.${action}`, parameters);
  }
}
