export const manifests: Array<UmbExtensionManifest> = [
  {
    name: "Dragonfly Umbraco 10 SiteAuditor Entrypoint",
    alias: "Dragonfly.SiteAuditor.Entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint.js"),
  },
];
