import { error } from './util/error.js';
import {
  AssignNode, BinaryNode, BinaryMode, BlockNode, BreakNode, CallNode, ContinueNode, ExpressionNode, ExpressionStatementNode,
  FuncParameterNode, FunctionDeclNode, IfNode, NumberLiteralNode, ReferenceNode, ReturnNode, StatementNode, SwitchNode, TypeRefNode,
  UnaryNode, UnaryMode, UnitNode, VariableDeclNode, WhileNode
} from './syntax-node.js';
import { Scanner } from './scan.js';
import { ITokenStream } from './stream/token-stream.js';
import { TokenKind } from './token.js';

export function parse(input: string): UnitNode {
  const s = new Scanner(input);

  const loc = s.getToken().loc;

  const decls = [];
  while (s.getKind() != TokenKind.EOF) {
    switch (s.getKind()) {
      case TokenKind.Fn:
      case TokenKind.External: {
        decls.push(parseFunctionDecl(s));
        break;
      }
      case TokenKind.Var: {
        decls.push(parseVariableDecl(s));
        break;
      }
      default: {
        throw error(`unexpected token: ${TokenKind[s.getKind()]}`, loc);
      }
    }
  }

  return new UnitNode(decls, loc);
}

function parseFuncParameters(s: ITokenStream): FuncParameterNode[] {
  s.nextWith(TokenKind.OpenParen);
  const items = [];
  while (s.getKind() != TokenKind.CloseParen) {
    if (items.length > 0) {
      s.nextWith(TokenKind.Comma);
    }

    const loc = s.getToken().loc;
    s.expect(TokenKind.Identifier);
    const name = s.getToken().value!;
    s.next();

    let typeRef;
    if (s.getKind() == TokenKind.Colon) {
      s.next();
      typeRef = parseTypeRef(s);
    }
    items.push(new FuncParameterNode(name, typeRef, loc));
  }
  s.nextWith(TokenKind.CloseParen);
  return items;
}

function parseCond(s: ITokenStream): ExpressionNode {
  s.nextWith(TokenKind.OpenParen);
  const expr = parseExpr(s);
  s.nextWith(TokenKind.CloseParen);
  return expr;
}

function parseBlock(s: ITokenStream): (StatementNode | ExpressionNode)[] {
  s.nextWith(TokenKind.OpenBrace);
  const steps = [];
  while (s.getKind() != TokenKind.CloseBrace) {
    steps.push(parseStep(s));
  }
  s.nextWith(TokenKind.CloseBrace);
  return steps;
}

function parseTypeRef(s: ITokenStream): TypeRefNode {
  const loc = s.getToken().loc;

  s.expect(TokenKind.Identifier);
  const name = s.getToken().value!;
  s.next();

  const suffixes: TypeRefNode['suffixes'] = [];
  while (true) {
    if (s.getKind() == TokenKind.Asterisk) {
      s.next();
      suffixes.push({ kind: 'pointer' });
    }
    else if (s.getKind() == TokenKind.OpenBracket) {
      s.next();
      const dimensions: { size: number | undefined }[] = [];
      while (s.getKind() != TokenKind.CloseBracket) {
        if (dimensions.length > 0) {
          s.nextWith(TokenKind.Comma);
        }
        const dimension: { size: number | undefined } = { size: undefined };
        if (s.getKind() == TokenKind.NumberLiteral) {
          const size = s.getToken().value!;
          dimension.size = Number(size);
          s.next();
        }
        dimensions.push(dimension);
      }
      s.nextWith(TokenKind.CloseBracket);
      suffixes.push({ kind: 'array', dimensions });
    }
    else {
      break;
    }
  }

  return new TypeRefNode(name, suffixes, loc);
}

function parseStep(s: ITokenStream): StatementNode | ExpressionNode {
  const loc = s.getToken().loc;
  let kind = s.getKind();

  // statement
  switch (kind) {
    case TokenKind.Break: {
      s.next();
      s.nextWith(TokenKind.SemiColon);
      return new BreakNode(loc);
    }
    case TokenKind.Continue: {
      s.next();
      s.nextWith(TokenKind.SemiColon);
      return new ContinueNode(loc);
    }
    case TokenKind.Return: {
      s.next();
      if (s.getKind() == TokenKind.SemiColon) {
        s.next();
        return new ReturnNode(undefined, loc);
      }
      const expr = parseExpr(s);
      s.nextWith(TokenKind.SemiColon);
      return new ReturnNode(expr, loc);
    }
    case TokenKind.Var: {
      return parseVariableDecl(s);
    }
    case TokenKind.While:
    case TokenKind.Do: {
      return parseWhile(s);
    }
  }

  // left expression
  const left = parseExpr(s);
  kind = s.getKind();

  // assign
  let assign;
  switch (kind) {
    case TokenKind.Eq: {
      s.next();
      const right = parseExpr(s);
      assign = new AssignNode('simple', left, right, loc);
      break;
    }
    case TokenKind.PlusEq: {
      s.next();
      const right = parseExpr(s);
      assign = new AssignNode('add', left, right, loc);
      break;
    }
    case TokenKind.MinusEq: {
      s.next();
      const right = parseExpr(s);
      assign = new AssignNode('sub', left, right, loc);
      break;
    }
    case TokenKind.AsterEq: {
      s.next();
      const right = parseExpr(s);
      assign = new AssignNode('mul', left, right, loc);
      break;
    }
    case TokenKind.SlashEq: {
      s.next();
      const right = parseExpr(s);
      assign = new AssignNode('div', left, right, loc);
      break;
    }
    case TokenKind.PercentEq: {
      s.next();
      const right = parseExpr(s);
      assign = new AssignNode('rem', left, right, loc);
      break;
    }
    case TokenKind.Lt2Eq: {
      s.next();
      const right = parseExpr(s);
      assign = new AssignNode('shl', left, right, loc);
      break;
    }
    case TokenKind.Gt2Eq: {
      s.next();
      const right = parseExpr(s);
      assign = new AssignNode('shr', left, right, loc);
      break;
    }
    case TokenKind.AndEq: {
      s.next();
      const right = parseExpr(s);
      assign = new AssignNode('bitand', left, right, loc);
      break;
    }
    case TokenKind.OrEq: {
      s.next();
      const right = parseExpr(s);
      assign = new AssignNode('bitor', left, right, loc);
      break;
    }
    case TokenKind.HatEq: {
      s.next();
      const right = parseExpr(s);
      assign = new AssignNode('xor', left, right, loc);
      break;
    }
  }
  if (assign != null) {
    s.nextWith(TokenKind.SemiColon);
    return assign;
  }

  // expression statement
  if (s.getKind() == TokenKind.SemiColon) {
    s.next();
    return new ExpressionStatementNode(left, loc);
  }

  // expression
  return left;
}

function parseExpr(s: ITokenStream): ExpressionNode {
  return parsePratt(s, 0);
}

function parseFunctionDecl(s: ITokenStream): FunctionDeclNode {
  const loc = s.getToken().loc;

  let external = false;
  if (s.getKind() == TokenKind.External) {
    external = true;
    s.next();
  }

  s.nextWith(TokenKind.Fn);

  s.expect(TokenKind.Identifier);
  const name = s.getToken().value!;
  s.next();

  const params = parseFuncParameters(s);

  let retTypeRef;
  if (s.getKind() == TokenKind.Colon) {
    s.next();
    retTypeRef = parseTypeRef(s);
  }

  let body;
  if (external) {
    s.nextWith(TokenKind.SemiColon);
  } else {
    body = parseBlock(s);
  }

  return new FunctionDeclNode(name, params, retTypeRef, body, external, loc);
}

function parseVariableDecl(s: ITokenStream): VariableDeclNode {
  const loc = s.getToken().loc;
  s.nextWith(TokenKind.Var);

  s.expect(TokenKind.Identifier);
  const name = s.getToken().value!;
  s.next();

  let typeRef;
  if (s.getKind() == TokenKind.Colon) {
    s.next();
    typeRef = parseTypeRef(s);
  }

  let body;
  if (s.getKind() == TokenKind.Eq) {
    s.next();
    body = parseExpr(s);
  }

  s.nextWith(TokenKind.SemiColon);
  return new VariableDeclNode(name, typeRef, body, loc);
}

function parseIf(s: ITokenStream): IfNode {
  const loc = s.getToken().loc;

  s.nextWith(TokenKind.If);
  const cond = parseCond(s);
  const thenExpr = parseExpr(s);
  let elseExpr;
  if (s.getKind() == TokenKind.Else) {
    s.next();
    elseExpr = parseExpr(s);
  }

  return new IfNode(cond, thenExpr, elseExpr, loc);
}

function parseSwitch(s: ITokenStream): SwitchNode {
  const loc = s.getToken().loc;

  s.nextWith(TokenKind.Switch);
  const expr = parseCond(s);
  s.nextWith(TokenKind.OpenBrace);
  const arms = [];
  let defaultBlock;
  while (s.getKind() != TokenKind.CloseBrace) {
    if (s.getKind() == TokenKind.Default) {
      s.next();
      const defaultArmLoc = s.getToken().loc;
      defaultBlock = new BlockNode(parseBlock(s), defaultArmLoc);
      break;
    }
    s.nextWith(TokenKind.Case);
    const armCond = parseExpr(s);
    const armLoc = s.getToken().loc;
    const armBlock = new BlockNode(parseBlock(s), armLoc);
    arms.push({ cond: armCond, thenBlock: armBlock });
  }
  s.nextWith(TokenKind.CloseBrace);

  return new SwitchNode(expr, arms, defaultBlock, loc);
}

function parseWhile(s: ITokenStream): WhileNode {
  const loc = s.getToken().loc;

  if (s.getKind() == TokenKind.While) {
    s.nextWith(TokenKind.While);
    const cond = parseCond(s);
    const body = parseBlock(s);
    return new WhileNode('while', cond, body, loc);
  } else {
    s.nextWith(TokenKind.Do);
    const body = parseBlock(s);
    s.nextWith(TokenKind.While);
    const cond = parseCond(s);
    return new WhileNode('do-while', cond, body, loc);
  }
}

//#region pratt parsing

type OpInfo = PrefixInfo | InfixInfo | PostfixInfo;

type PrefixInfo = { kind: 'prefix', token: PrefixToken, bp: number };
const prefixOp = (token: PrefixToken, bp: number): OpInfo => ({ kind: 'prefix', token, bp });

type PrefixToken =
  | TokenKind.Not
  | TokenKind.Plus
  | TokenKind.Minus;

type InfixInfo = { kind: 'infix', token: InfixToken, lbp: number, rbp: number };
const infixOp = (token: InfixToken, lbp: number, rbp: number): OpInfo => ({ kind: 'infix', token, lbp, rbp });

type InfixToken =
  //| TokenKind.Dot
  | TokenKind.Asterisk
  | TokenKind.Slash
  | TokenKind.Percent
  | TokenKind.Plus
  | TokenKind.Minus
  | TokenKind.Lt
  | TokenKind.LtEq
  | TokenKind.Gt
  | TokenKind.GtEq
  | TokenKind.Eq2
  | TokenKind.NotEq
  | TokenKind.And2
  | TokenKind.Or2;

type PostfixInfo = { kind: 'postfix', token: PostfixToken, bp: number };
const postfixOp = (token: PostfixToken, bp: number): OpInfo => ({ kind: 'postfix', token, bp });

type PostfixToken =
  //| TokenKind.OpenBracket
  | TokenKind.OpenParen;

const operators: OpInfo[] = [
  postfixOp(TokenKind.OpenParen, 90),
  //postfixOp(TokenKind.OpenBracket, 90),
  //infixOp(TokenKind.Dot, 90, 91),
  prefixOp(TokenKind.Not, 80),
  prefixOp(TokenKind.Plus, 80),
  prefixOp(TokenKind.Minus, 80),
  infixOp(TokenKind.Asterisk, 70, 71),
  infixOp(TokenKind.Slash, 70, 71),
  infixOp(TokenKind.Percent, 70, 71),
  infixOp(TokenKind.Plus, 60, 61),
  infixOp(TokenKind.Minus, 60, 61),
  infixOp(TokenKind.Lt, 50, 51),
  infixOp(TokenKind.LtEq, 50, 51),
  infixOp(TokenKind.Gt, 50, 51),
  infixOp(TokenKind.GtEq, 50, 51),
  infixOp(TokenKind.Eq2, 40, 41),
  infixOp(TokenKind.NotEq, 40, 41),
  infixOp(TokenKind.And2, 30, 31),
  infixOp(TokenKind.Or2, 20, 21),
];

function parsePratt(s: ITokenStream, minBp: number): ExpressionNode {
  // pratt parsing
  // https://matklad.github.io/2020/04/13/simple-but-powerful-pratt-parsing.html
  const kind = s.getKind();
  const prefix = operators.find((x): x is PrefixInfo => x.kind == 'prefix' && x.token == kind);
  let left: ExpressionNode;
  if (prefix != null) {
    // prefix
    left = parsePrefix(s, prefix);
  } else {
    left = parseAtom(s);
  }
  while (true) {
    const kind = s.getKind();
    const postfix = operators.find((x): x is PostfixInfo => x.kind == 'postfix' && x.token == kind);
    if (postfix != null) {
      // postfix
      if (postfix.bp < minBp) {
        break;
      }
      left = parsePostfix(s, left, postfix);
      continue;
    }
    const infix = operators.find((x): x is InfixInfo => x.kind == 'infix' && x.token == kind);
    if (infix != null) {
      // infix
      if (infix.lbp < minBp) {
        break;
      }
      left = parseInfix(s, left, infix);
      continue;
    }
    break;
  }
  return left;
}

function parsePrefix(s: ITokenStream, info: PrefixInfo): ExpressionNode {
  const loc = s.getToken().loc;
  s.next();
  const right = parsePratt(s, info.bp);

  let mode: UnaryMode;
  switch (info.token) {
    case TokenKind.Not: {
      mode = 'not';
      break;
    }
    case TokenKind.Plus: {
      mode = 'plus';
      break;
    }
    case TokenKind.Minus: {
      mode = 'minus';
      break;
    }
  }

  return new UnaryNode(mode, right, loc);
}

function parsePostfix(s: ITokenStream, left: ExpressionNode, info: PostfixInfo): ExpressionNode {
  const loc = s.getToken().loc;
  s.next();
  switch (info.token) {
    // case TokenKind.OpenBracket: {
    //   // index access
    //   const index = parseExpr(s);
    //   s.nextWith(TokenKind.CloseBracket);
    //   return createIndexAccess(loc, left, index);
    // }
    case TokenKind.OpenParen: {
      // call
      const args: ExpressionNode[] = [];
      if (s.getKind() != TokenKind.CloseParen) {
        args.push(parseExpr(s));
        while (s.getKind() == (TokenKind.Comma)) {
          s.next();
          if (s.getKind() == (TokenKind.CloseParen)) {
            break;
          }
          args.push(parseExpr(s));
        }
      }
      s.nextWith(TokenKind.CloseParen);
      return new CallNode(left, args, loc);
    }
  }
}

function parseInfix(s: ITokenStream, left: ExpressionNode, info: InfixInfo): ExpressionNode {
  const loc = s.getToken().loc;
  s.next();
  const right = parsePratt(s, info.rbp);
  // if (info.token == TokenKind.Dot) {
  //   // field access
  //   if (right.kind !== 'Reference') {
  //     throw new Error(`Reference is expected. ${right.loc.line + 1}:${right.loc.column + 1}`);
  //   }
  //   return createFieldAccess(loc, right.name, left);
  // }

  let mode: BinaryMode;
  switch (info.token) {
    // case TokenKind.Dot: {
    //   mode = '';
    //   break;
    // }
    case TokenKind.Asterisk: {
      mode = 'mul';
      break;
    }
    case TokenKind.Slash: {
      mode = 'div';
      break;
    }
    case TokenKind.Percent: {
      mode = 'rem';
      break;
    }
    case TokenKind.Plus: {
      mode = 'add';
      break;
    }
    case TokenKind.Minus: {
      mode = 'sub';
      break;
    }
    case TokenKind.Lt: {
      mode = 'lt';
      break;
    }
    case TokenKind.LtEq: {
      mode = 'lte';
      break;
    }
    case TokenKind.Gt: {
      mode = 'gt';
      break;
    }
    case TokenKind.GtEq: {
      mode = 'gte';
      break;
    }
    case TokenKind.Eq2: {
      mode = 'eq';
      break;
    }
    case TokenKind.NotEq: {
      mode = 'neq';
      break;
    }
    case TokenKind.And2: {
      mode = 'and';
      break;
    }
    case TokenKind.Or2: {
      mode = 'or';
      break;
    }
  }

  return new BinaryNode(mode, left, right, loc);
}

/**
 * ```text
 * <Atom> = <NumberLiteral> / <BoolLiteral> / <StringLiteral> / <StructExpr> / <Array> / <IfExpr> / <Identifier> / "(" <Expr> ")"
 * ```
*/
function parseAtom(s: ITokenStream): ExpressionNode {
  const loc = s.getToken().loc;
  switch (s.getKind()) {
    case TokenKind.NumberLiteral: {
      const source = s.getToken().value!;
      s.next();
      const value = Number(source);
      return new NumberLiteralNode(value, loc);
    }
    case TokenKind.Identifier: {
      const name = s.getToken().value!;
      s.next();
      return new ReferenceNode(name, loc);
    }
    // case TokenKind.New: {
    //   s.next();
    //   s.expect(TokenKind.Identifier);
    //   const name = s.getIdentValue();
    //   s.next();
    //   s.nextWith(TokenKind.OpenBrace);
    //   const fields: StructExprField[] = [];
    //   if (s.getKind() != (TokenKind.CloseBrace)) {
    //     fields.push(parseStructExprField(s));
    //     while (s.getKind() == (TokenKind.Comma)) {
    //       s.next();
    //       if (s.getKind() == (TokenKind.CloseBrace)) {
    //         break;
    //       }
    //       fields.push(parseStructExprField(s));
    //     }
    //   }
    //   s.nextWith(TokenKind.CloseBrace);
    //   return createStructExpr(loc, name, fields);
    // }
    case TokenKind.If: {
      return parseIf(s);
    }
    case TokenKind.Switch: {
      return parseSwitch(s);
    }
    case TokenKind.OpenBrace: {
      return new BlockNode(parseBlock(s), loc);
    }
    case TokenKind.OpenParen: {
      s.next();
      const expr = parseExpr(s);
      s.nextWith(TokenKind.CloseParen);
      return expr;
    }
    default: {
      throw error(`unexpected token: ${TokenKind[s.getKind()]}`, loc);
    }
  }
}

//#endregion pratt parsing
