import { html as r, css as h, customElement as m } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement as p } from "@umbraco-cms/backoffice/lit-element";
var g = Object.getOwnPropertyDescriptor, c = (e) => {
  throw TypeError(e);
}, y = (e, a, t, s) => {
  for (var o = s > 1 ? void 0 : s ? g(a, t) : a, n = e.length - 1, d; n >= 0; n--)
    (d = e[n]) && (o = d(o) || o);
  return o;
}, A = (e, a, t) => a.has(e) || c("Cannot " + t), D = (e, a, t) => a.has(e) ? c("Cannot add the same private member more than once") : a instanceof WeakSet ? a.add(e) : a.set(e, t), u = (e, a, t) => (A(e, a, "access private method"), t), l, f, b;
const v = [
  {
    heading: "Content",
    note: 'Choosing "Only Published" is faster and less resource-intensive.',
    links: [
      { label: "All Content Nodes", href: "/umbraco/backoffice/Dragonfly/SiteAuditor/GetAllContentAsHtmlTable" },
      { label: "Only Published", href: "/umbraco/backoffice/Dragonfly/SiteAuditor/GetAllContentAsHtmlTable?PublishedOnly=true" },
      { label: "Content Nodes For Doctype", href: "/umbraco/backoffice/Dragonfly/SiteAuditor/GetContentForDoctypeHtml" },
      { label: "Content Nodes with Property Data", href: "/umbraco/backoffice/Dragonfly/SiteAuditor/GetContentWithValues" }
    ]
  },
  {
    heading: "Media",
    links: [
      { label: "All Media Nodes", href: "/umbraco/backoffice/Dragonfly/SiteAuditor/GetAllMediaAsHtmlTable" },
      { label: "All Media with Thumbnails", href: "/umbraco/backoffice/Dragonfly/SiteAuditor/GetAllMediaAsHtmlTable?ShowImageThumbnails=true" },
      { label: "Media Nodes For MediaType", href: "/umbraco/backoffice/Dragonfly/SiteAuditor/GetMediaForTypeHtml" },
      { label: "Media Nodes with Property Data", href: "/umbraco/backoffice/Dragonfly/SiteAuditor/GetMediaWithValues" }
    ]
  },
  {
    heading: "Document Types",
    links: [
      { label: "All Document Types", href: "/umbraco/backoffice/Dragonfly/SiteAuditor/GetAllDocTypesAsHtmlTable" },
      { label: "All DocType Properties", href: "/umbraco/backoffice/Dragonfly/SiteAuditor/GetAllPropertiesAsHtmlTable" },
      { label: "Single DocType's Properties", href: "/umbraco/backoffice/Dragonfly/SiteAuditor/GetPropertiesForDoctypeHtml" },
      { label: "Content Using an Element Type", href: "/umbraco/backoffice/Dragonfly/SiteAuditor/GetContentForElementType" }
    ]
  },
  {
    heading: "Data Types",
    links: [
      { label: "All DataTypes", href: "/umbraco/backoffice/Dragonfly/SiteAuditor/GetAllDataTypesAsHtmlTable" }
    ]
  },
  {
    heading: "Templates",
    links: [
      { label: "All Templates", href: "/umbraco/backoffice/Dragonfly/SiteAuditor/GetAllTemplatesAsHtmlTable" },
      { label: "Template Usage Report", href: "/umbraco/backoffice/Dragonfly/SiteAuditor/TemplateUsageReport" }
    ]
  },
  {
    heading: "Logs",
    links: [
      { label: "Log Entries", href: "/umbraco/backoffice/Dragonfly/SiteAuditor/GetLogs" }
    ]
  }
];
let i = class extends p {
  constructor() {
    super(...arguments), D(this, l);
  }
  render() {
    return r`
      <div class="dashboard-header">
        <div>
          <h2 class="dashboard-title">Dragonfly Site Auditor</h2>
          <p class="dashboard-notice">
            <strong>NOTE:</strong> Some of these will take a long time to run for large sites. Please be patient.
          </p>
        </div>
        <img
          src="/App_Plugins/Dragonfly.SiteAuditor/Dragonfly-SiteAuditor-128.png"
          alt="Dragonfly Site Auditor"
          class="dashboard-logo"
        />
      </div>

      <div class="sections-grid">
        ${v.map((e) => u(this, l, b).call(this, e))}
      </div>

      <p class="dashboard-footer">
        <small>
          For more information and updates:
          <a href="https://github.com/hfloyd/Dragonfly.Umbraco10.SiteAuditor" target="_blank" rel="noopener noreferrer">
            Dragonfly.Umbraco10.SiteAuditor
          </a>
        </small>
      </p>
    `;
  }
};
l = /* @__PURE__ */ new WeakSet();
f = function(e) {
  return r`
      <li>
        <a href=${e.href} target="_blank" rel="noopener noreferrer">${e.label}</a>
      </li>
    `;
};
b = function(e) {
  return r`
      <uui-box headline=${e.heading}>
        ${e.note ? r`<p class="section-note">${e.note}</p>` : ""}
        <ul class="link-list">
          ${e.links.map((a) => u(this, l, f).call(this, a))}
        </ul>
      </uui-box>
    `;
};
i.styles = [
  h`
      :host {
        display: block;
        padding: var(--uui-size-layout-1);
      }

      .dashboard-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        margin-bottom: var(--uui-size-layout-1);
      }

      .dashboard-title {
        margin: 0 0 var(--uui-size-space-3) 0;
        font-size: var(--uui-type-h4-size);
      }

      .dashboard-notice {
        margin: 0;
        font-style: italic;
        color: var(--uui-color-text-alt);
      }

      .dashboard-logo {
        width: 80px;
        height: 80px;
        flex-shrink: 0;
      }

      .sections-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
        gap: var(--uui-size-layout-1);
      }

      .section-note {
        margin: 0 0 var(--uui-size-space-3) 0;
        font-size: var(--uui-type-small-size);
        color: var(--uui-color-text-alt);
      }

      .link-list {
        list-style: none;
        margin: 0;
        padding: 0;
        display: flex;
        flex-direction: column;
        gap: var(--uui-size-space-3);
      }

      .link-list li a {
        color: var(--uui-color-interactive);
        text-decoration: none;
      }

      .link-list li a:hover {
        color: var(--uui-color-interactive-emphasis);
        text-decoration: underline;
      }

      .dashboard-footer {
        margin-top: var(--uui-size-layout-1);
        color: var(--uui-color-text-alt);
      }

      .dashboard-footer a {
        color: var(--uui-color-interactive);
      }
    `
];
i = y([
  m("dragonfly-site-auditor-dashboard")
], i);
const T = i;
export {
  i as DragonflySiteAuditorDashboardElement,
  T as default
};
//# sourceMappingURL=dashboard.element-DqXKNLAD.js.map
