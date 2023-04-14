
let assemblies = null;

async function sendRequest(type, request) {
    let endPoint;
    const headers = {
        'Content-Type': 'application/json'
    };
    switch (type) {
        case 'complete': endPoint = '/completion/complete'; break;
        case 'signature': endPoint = '/completion/signature'; break;
        case 'hover': endPoint = '/completion/hover'; break;
        case 'codeCheck': endPoint = '/completion/codeCheck'; break;
    }
    // 添加请求拦截器
    axios.interceptors.request.use(request => {
        console.log('Starting Request', request)
        return request
    });

    const response = await axios.post(endPoint, request);
    console.log(response)
    return response.data;
}

export async function provideHover(model, position) {
    let request = {
        SourceCode: model.getValue(),
        Position: model.getOffsetAt(position),
        Assemblies: assemblies
    }

    let resultQ = await sendRequest("hover", request);
    console.log(resultQ);
    console.log(resultQ.data);
    console.log("registerHoverProvider");
    if (resultQ.data) {
        posStart = model.getPositionAt(resultQ.data.OffsetFrom);
        posEnd = model.getPositionAt(resultQ.data.OffsetTo);

        return {
            range: new monaco.Range(posStart.lineNumber, posStart.column, posEnd.lineNumber, posEnd.column),
            contents: [
                { value: resultQ.data.Information }
            ]
        };
    }

    return null;
}
export async function provideSignatureHelp(model, postion) {
    let request = {
        SourceCode: model.getValue(),
        Position: model.getOffsetAt(position),
        Assemblies: assemblies
    }

    let resultQ = await sendRequest("signature", request);
    if (!resultQ.data) return;
    console.log("sss");
    console.log(resultQ);
    let signatures = [];
    for (let signature of resultQ.data.Signatures) {
        let params = [];
        for (let param of signature.Parameters) {
            params.push({
                label: param.Label,
                documentation: param.Documentation ?? ""
            });
        }

        signatures.push({
            label: signature.Label,
            documentation: signature.Documentation ?? "",
            parameters: params,
        });
    }

    let signatureHelp = {};
    signatureHelp.signatures = signatures;
    signatureHelp.activeParameter = resultQ.data.ActiveParameter;
    signatureHelp.activeSignature = resultQ.data.ActiveSignature;

    return {
        value: signatureHelp,
        dispose: () => { }
    };
}

export async function provideCompletionItems(model, position){
    let suggestions = [];

    let request = {
        SourceCode: model.getValue(),
        Position: model.getOffsetAt(position),
        Assemblies: assemblies
    }
    console.log("provideCompletionItems")

    let resultQ = await sendRequest("complete", request);

    console.log(resultQ);
    for (let elem of resultQ.data) {
        suggestions.push({
            label: {
                label: elem.Suggestion,
                description: elem.Description
            },
            kind: monaco.languages.CompletionItemKind.Function,
            insertText: elem.Suggestion

        });
    }

    return { suggestions: suggestions };
}

//提示信息
export async function validate() {

    let request = {
        SourceCode: model.getValue(),
        Assemblies: assemblies
    }

    let resultQ = await sendRequest("codeCheck", request)

    let markers = [];

    for (let elem of resultQ.data) {
        posStart = model.getPositionAt(elem.OffsetFrom);
        posEnd = model.getPositionAt(elem.OffsetTo);
        markers.push({
            severity: elem.Severity,
            startLineNumber: posStart.lineNumber,
            startColumn: posStart.column,
            endLineNumber: posEnd.lineNumber,
            endColumn: posEnd.column,
            message: elem.Message,
            code: elem.Id
        });
    }
    console.log(markers);
    console.log("onDidCreateModel");
    monaco.editor.setModelMarkers(model, 'csharp', markers);
}

export const languageId = 'csharp';

/**
 * 
 */
export const Monarch = {
    defaultToken: 'invalid',
    tokenPostfix: '.cs',

    brackets: [
        { open: '{', close: '}', token: 'delimiter.curly' },
        { open: '[', close: ']', token: 'delimiter.square' },
        { open: '(', close: ')', token: 'delimiter.parenthesis' },
        { open: '<', close: '>', token: 'delimiter.angle' }
    ],

    keywords: [
        'extern', 'alias', 'using', 'bool', 'decimal', 'sbyte', 'byte', 'short',
        'ushort', 'int', 'uint', 'long', 'ulong', 'char', 'float', 'double',
        'object', 'dynamic', 'string', 'assembly', 'is', 'as', 'ref',
        'out', 'this', 'base', 'new', 'typeof', 'void', 'checked', 'unchecked',
        'default', 'delegate', 'var', 'const', 'if', 'else', 'switch', 'case',
        'while', 'do', 'for', 'foreach', 'in', 'break', 'continue', 'goto',
        'return', 'throw', 'try', 'catch', 'finally', 'lock', 'yield', 'from',
        'let', 'where', 'join', 'on', 'equals', 'into', 'orderby', 'ascending',
        'descending', 'select', 'group', 'by', 'namespace', 'partial', 'class',
        'field', 'event', 'method', 'param', 'property', 'public', 'protected',
        'internal', 'private', 'abstract', 'sealed', 'static', 'struct', 'readonly',
        'volatile', 'virtual', 'override', 'params', 'get', 'set', 'add', 'remove',
        'operator', 'true', 'false', 'implicit', 'explicit', 'interface', 'enum',
        'null', 'async', 'await', 'fixed', 'sizeof', 'stackalloc', 'unsafe', 'nameof',
        'when'
    ],

    typeKeywords: [
        'boolean', 'double', 'byte', 'int', 'short', 'char', 'void', 'long', 'float'
    ],

    namespaceFollows: [
        'namespace', 'using',
    ],

    parenFollows: [
        'if', 'for', 'while', 'switch', 'foreach', 'using', 'catch', 'when'
    ],

    operators: [
        '=', '??', '||', '&&', '|', '^', '&', '==', '!=', '<=', '>=', '<<',
        '+', '-', '*', '/', '%', '!', '~', '++', '--', '+=',
        '-=', '*=', '/=', '%=', '&=', '|=', '^=', '<<=', '>>=', '>>', '=>'
    ],

    symbols: /[=><!~?:&|+\-*\/\^%]+/,

    // escape sequences
    escapes: /\\(?:[abfnrtv\\"']|x[0-9A-Fa-f]{1,4}|u[0-9A-Fa-f]{4}|U[0-9A-Fa-f]{8})/,

    // The main tokenizer for our languages
    tokenizer: {
        root: [

            // identifiers and keywords
            [/\@?[a-zA-Z_]\w*/, {
                cases: {
                    '@namespaceFollows': { token: 'keyword.$0', next: '@namespace' },
                    '@keywords': { token: 'keyword.$0', next: '@qualified' },
                    '@default': { token: 'identifier', next: '@qualified' }
                }
            }],

            // whitespace
            { include: '@whitespace' },

            // delimiters and operators
            [/}/, {
                cases: {
                    '$S2==interpolatedstring': { token: 'string.quote', next: '@pop' },
                    '$S2==litinterpstring': { token: 'string.quote', next: '@pop' },
                    '@default': '@brackets'
                }
            }],
            [/[{}()\[\]]/, '@brackets'],
            [/[<>](?!@symbols)/, '@brackets'],
            [/@symbols/, {
                cases: {
                    '@operators': 'delimiter',
                    '@default': ''
                }
            }],

            // numbers
            [/[0-9_]*\.[0-9_]+([eE][\-+]?\d+)?[fFdD]?/, 'number.float'],
            [/0[xX][0-9a-fA-F_]+/, 'number.hex'],
            [/0[bB][01_]+/, 'number.hex'], // binary: use same theme style as hex
            [/[0-9_]+/, 'number'],

            // delimiter: after number because of .\d floats
            [/[;,.]/, 'delimiter'],

            // strings
            [/"([^"\\]|\\.)*$/, 'string.invalid'],  // non-teminated string
            [/"/, { token: 'string.quote', next: '@string' }],
            [/\$\@"/, { token: 'string.quote', next: '@litinterpstring' }],
            [/\@"/, { token: 'string.quote', next: '@litstring' }],
            [/\$"/, { token: 'string.quote', next: '@interpolatedstring' }],

            // characters
            [/'[^\\']'/, 'string'],
            [/(')(@escapes)(')/, ['string', 'string.escape', 'string']],
            [/'/, 'string.invalid']
        ],

        qualified: [
            [/[a-zA-Z_][\w]*/, {
                cases: {
                    '@keywords': { token: 'keyword.$0' },
                    '@default': 'identifier'
                }
            }],
            [/\./, 'delimiter'],
            ['', '', '@pop'],
        ],

        namespace: [
            { include: '@whitespace' },
            [/[A-Z]\w*/, 'namespace'],
            [/[\.=]/, 'delimiter'],
            ['', '', '@pop'],
        ],

        comment: [
            [/[^\/*]+/, 'comment'],
            // [/\/\*/,    'comment', '@push' ],    // no nested comments :-(
            ['\\*/', 'comment', '@pop'],
            [/[\/*]/, 'comment']
        ],

        string: [
            [/[^\\"]+/, 'string'],
            [/@escapes/, 'string.escape'],
            [/\\./, 'string.escape.invalid'],
            [/"/, { token: 'string.quote', next: '@pop' }]
        ],

        litstring: [
            [/[^"]+/, 'string'],
            [/""/, 'string.escape'],
            [/"/, { token: 'string.quote', next: '@pop' }]
        ],

        litinterpstring: [
            [/[^"{]+/, 'string'],
            [/""/, 'string.escape'],
            [/{{/, 'string.escape'],
            [/}}/, 'string.escape'],
            [/{/, { token: 'string.quote', next: 'root.litinterpstring' }],
            [/"/, { token: 'string.quote', next: '@pop' }]
        ],

        interpolatedstring: [
            [/[^\\"{]+/, 'string'],
            [/@escapes/, 'string.escape'],
            [/\\./, 'string.escape.invalid'],
            [/{{/, 'string.escape'],
            [/}}/, 'string.escape'],
            [/{/, { token: 'string.quote', next: 'root.interpolatedstring' }],
            [/"/, { token: 'string.quote', next: '@pop' }]
        ],

        whitespace: [
            [/^[ \t\v\f]*#((r)|(load))(?=\s)/, 'directive.csx'],
            [/^[ \t\v\f]*#\w.*$/, 'namespace.cpp'],
            [/[ \t\v\f\r\n]+/, ''],
            [/\/\*/, 'comment', '@comment'],
            [/\/\/.*$/, 'comment'],
        ],
    },
};

export const languageConfiguration = {

    comments: {
        lineComment: '//',
        blockComment: ['/*', '*/'],
    },
    brackets: [
        ['{', '}'],
        ['[', ']'],
        ['(', ')']
    ],
    autoClosingPairs: [
        { open: '{', close: '}' },
        { open: '[', close: ']' },
        { open: '(', close: ')' },
        { open: '\'', close: '\'', notIn: ['string', 'comment'] },
        { open: '\"', close: '\"', notIn: ['string'] }
    ],
    surroundingPairs: [
        { open: '{', close: '}' },
        { open: '[', close: ']' },
        { open: '(', close: ')' },
        { open: '\'', close: '\'' },
        { open: '\"', close: '\"' }
    ],
    indentationRules: {
        decreaseIndentPattern: /^((?!.*?\/\*).*\*\/)?\s*[\}\]\)].*$/,
        increaseIndentPattern: /^((?!\/\/).)*(\{[^}"'`]*|\([^)"'`]*|\[[^\]"'`]*)$/
    }
}

export const legend = {
    tokenTypes: [
        "comment",
        "string",
        "keyword",
        "number",
        "regexp",
        "operator",
        "namespace",
        "type",
        "struct",
        "class",
        "interface",
        "enum",
        "typeParameter",
        "function",
        "member",
        "macro",
        "variable",
        "parameter",
        "property",
        "label",
    ],
    tokenModifiers: [
        "declaration",
        "documentation",
        "readonly",
        "static",
        "abstract",
        "deprecated",
        "modification",
        "async",
    ],
}

const CSHARP_TOKENS = {
    CLASS: "class",
    METHOD: "method",
    PROPERTY: "property",
    COMMENT: "comment",
    STRING: "string",
    NUMBER: "number",
    KEYWORD: "keyword",
    IDENTIFIER: "identifier",
};
const CSHARP_LEGEND = {
    tokenTypes: [
        CSHARP_TOKENS.CLASS,
        CSHARP_TOKENS.METHOD,
        CSHARP_TOKENS.PROPERTY,
        CSHARP_TOKENS.COMMENT,
        CSHARP_TOKENS.STRING,
        CSHARP_TOKENS.NUMBER,
        CSHARP_TOKENS.KEYWORD,
        CSHARP_TOKENS.IDENTIFIER,
    ],
    tokenModifiers: ["static", "readonly", "async"],
    colors: {
        [CSHARP_TOKENS.CLASS]: "#569cd6",
        [CSHARP_TOKENS.METHOD]: "#c586c0",
        [CSHARP_TOKENS.PROPERTY]: "#9cdcfe",
        [CSHARP_TOKENS.COMMENT]: "#6a9955",
        [CSHARP_TOKENS.STRING]: "#ce9178",
        [CSHARP_TOKENS.NUMBER]: "#b5cea8",
        [CSHARP_TOKENS.KEYWORD]: "#4ec9b0",
        [CSHARP_TOKENS.IDENTIFIER]: "#dcdcaa",
    },
};
const CSHARP_TOKEN_PATTERN = /(?<=\W|^)(class|void|int|string|bool|true|false|null|var|new|async|await|namespace|using)(?=\W|$)|(?<=\W|^)(static|readonly)(\.[A-Za-z0-9_]+)*(?=\W|$)|(?<=\W|^)(\w+)(?= *\()|\/\/.*|\/\*[\s\S]*?\*\//gm;


export function  getType(type) {
    return legend.tokenTypes.indexOf(type);
}

/** 获取mod
 *  @type {(modifier: string[]|string|null)=>number} 
 * @param grid {Ext.Grid.Panel} 需要合并的Grid
* @param cols {Array} 需要合并列的Index(序号)数组；从0开始计数，序号也包含。
* @param isAllSome {Boolean} ：是否2个tr的cols必须完成一样才能进行合并。true：完成一样；false(默认)：不完全一样
* @return void
* @author polk6 2015/07/21 
* @example
* _________________                             _________________
* |  年龄 |  姓名 |                             |  年龄 |  姓名 |
* -----------------      mergeCells(grid,[0])   -----------------
* |  18   |  张三 |              =>             |       |  张三 |
* -----------------                             -  18   ---------
* |  18   |  王五 |                             |       |  王五 |
* -----------------                             -----------------
 */
export function getModifier(modifiers) {
    if (typeof modifiers === "string") {
        modifiers = [modifiers];
    }
    if (Array.isArray(modifiers)) {
        let nModifiers = 0;
        for (let modifier of modifiers) {
            const nModifier = legend.tokenModifiers.indexOf(modifier);
            if (nModifier > -1) {
                nModifiers |= (1 << nModifier) >>> 0;
            }
        }
        return nModifiers;
    } else {
        return 0;
    }
}
export function registerDocumentSemanticTokensProvider() { 
monaco.languages.registerDocumentSemanticTokensProvider(languageId, {
    getLegend: function () { return CSHARP_LEGEND; },
    provideDocumentSemanticTokens: function (model, lastResultId, token) {

        if (lastResultId) {
            let cachedResult = cache.get(lastResultId);
            if (cachedResult) {
                return cachedResult;
            }
        }

        var lines = model.getLinesContent();
        var data = [];

        let result = computeDocumentSemanticTokens(model, token);

        // 将结果添加到缓存中
        let resultId = uuidv4();
        cache.set(resultId, result);

        return {
            data: result.tokens,
            resultId,
        };
    },
    releaseDocumentSemanticTokens: function (resultId) {
        // Release any resources associated with the given resultId here...
        cache.delete(resultId);
    }
});
function computeDocumentSemanticTokens(model, token) {
    let lines = model.getLinesContent();

    let tokens = [];
    let offset = 0;

    for (let i = 0; i < lines.length; i++) {
        let line = lines[i];

        let lineTokens = computeLineSemanticTokens(line, offset, token);
        tokens = tokens.concat(lineTokens);
        offset += line.length + 1; // +1 是因为每行末尾会有一个换行符
    }

    return { tokens };
}

function computeLineSemanticTokens(line, offset, token) {
    let tokens = [];

    // ... 根据语法规则计算出该行的语义单元 ...

    return tokens;
}

let cache = new Map();
}


//弹窗
function createPopup() {
    const popup = document.createElement('div');
    popup.style.position = 'fixed';
    popup.style.top = '5%'; // 垂直方向上占据页面 90%
    popup.style.left = '5%';
    popup.style.width = '90%'; // 水平方向上占据页面 90%
    popup.style.height = '90%';
    popup.style.backgroundColor = '#fff'; // 不透明白色背景

    const frame = document.createElement('iframe');
    frame.style.width = '100%';
    frame.style.height = '100%';
    frame.src = 'https://juejin.cn/post/6984683777343619102';
    frame.setAttribute('tabindex', '0'); // 将 iframe 设为可聚焦元素
    popup.appendChild(frame);

    const closeButton = document.createElement('button');
    closeButton.style.position = 'absolute';
    closeButton.style.top = '10px';
    closeButton.style.right = '10px';
    closeButton.style.color = '#000';
    closeButton.style.fontSize = '20px';
    closeButton.innerText = '关闭';
    closeButton.addEventListener('click', () => {
        document.body.removeChild(popup);
    });
    popup.appendChild(closeButton);

    document.body.appendChild(popup);

    // 将焦点定向到 iframe 中
    frame.focus();

    // 绑定按键事件，按 ESC 关闭弹窗
    document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape') {
            document.body.removeChild(popup);
        }
    });

    // 绑定事件，当弹窗内的元素失去焦点时，将焦点重新定向到 iframe 中
    const elements = popup.querySelectorAll('*');
    elements.forEach((element) => {
        element.addEventListener('blur', () => {
            frame.focus();
        });
    });
}