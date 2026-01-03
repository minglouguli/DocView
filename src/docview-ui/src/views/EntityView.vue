<script setup>
import {
  computed,
  nextTick,
  onMounted,
  onUnmounted,
  ref,
  useTemplateRef,
  watch
} from 'vue';
import { jsPlumb } from 'jsplumb';
import jsonData from '@/datas/jsondoc';
import IconCenter from '@/components/icons/IconCenter.vue';
import IconSearch from '@/components/icons/IconSearch.vue';
import IconArrowDown from '@/components/icons/IconArrowDown.vue';
import IconArrowRight from '@/components/icons/IconArrowRight.vue';

defineOptions({ name: 'EntityView' });

const jsPlumbContainerRef = useTemplateRef('jsPlumbContainerRef');
const jsPlumbContainerWrapperRef = useTemplateRef('jsPlumbContainerWrapperRef');
const nodesRef = useTemplateRef('nodesRef');

const count = ref(0);

const options = {
  colSpan: 200,
  with: 2000,
  span: 20,
  height: 2000,
  viewWith: '100%',
  titleHeight: 60,
  propertyHeight: 20,
  relationLevel: 3
};

const containerOpt = ref({
  left: 0,
  top: 0,
  zoom: 1,
  targetNode: null,
  targetNodeLeft: 0,
  targetNodeTop: 0
});

watch(
  containerOpt,
  () => {
    jsPlumbContainerRef.value.style.transform = `translate(${containerOpt.value.left}px, ${containerOpt.value.top}px) scale(${containerOpt.value.zoom})`;
  },
  {
    deep: true
  }
);

const data = ref({});

const json = ref({});

function initNodes(nodes) {
  // 最小间距
  const minSpacing = 60;
  // 初始化节点尺寸和位置
  nodes.forEach(node => {
    // 如果未定义宽高，使用默认值
    node.width = getNodeWidth(node) + 40; // node.name.length * 14 + 20 // // node.width ? parseInt(node.width) : 150

    node.height =
      options.titleHeight +
      (node.properties?.filter(f => f.isShow)?.length ?? 0) *
        options.propertyHeight; // node.height ? (node.height === 'auto' ? 120 : parseInt(node.height)) : 120

    node.left = 0;
    node.top = 0;
  });
  // 如果没有节点，直接返回
  if (nodes.length === 0) return;

  // 创建节点映射以便快速查找
  const nodeMap = {};
  nodes.forEach(node => {
    nodeMap[node.id] = node;
  });
  // 构建节点关系图
  const relations = {};
  json.value.connects.forEach(connect => {
    const sourceNodeId = connect.sourceNode;
    const targetNodeId = connect.targetNode;

    if (!relations[sourceNodeId]) relations[sourceNodeId] = [];
    if (!relations[targetNodeId]) relations[targetNodeId] = [];

    relations[sourceNodeId].push(targetNodeId);
    relations[targetNodeId].push(sourceNodeId);
  });
  // 检查两个节点是否重叠（包括间距）
  function isOverlapping(node1, node2) {
    return (
      node1.left < node2.left + node2.width + minSpacing &&
      node1.left + node1.width + minSpacing > node2.left &&
      node1.top < node2.top + node2.height + minSpacing &&
      node1.top + node1.height + minSpacing > node2.top
    );
  }

  // 检查新位置是否与已放置节点重叠
  function checkOverlapWithPlaced(newNode, exceptNodeId = null) {
    for (const node of nodes) {
      if (node.id === exceptNodeId) continue;
      // 只检查已经设置了位置的节点
      if (
        node.left !== undefined &&
        node.top !== undefined &&
        node.left !== 0 &&
        node.top !== 0
      ) {
        if (isOverlapping(newNode, node)) {
          return true;
        }
      }
    }
    return false;
  }

  // 获取与指定节点相关的已放置节点
  function getConnectedPlacedNodes(nodeId) {
    return (relations[nodeId] || [])
      .map(id => nodeMap[id])
      .filter(
        node =>
          node.left !== undefined &&
          node.top !== undefined &&
          (node.left !== 0 || node.top !== 0)
      );
  }

  // 首先放置第一个节点在画布中心
  nodes[0].left = options.with / 2 - nodes[0].width / 2;
  nodes[0].top = options.height / 2 - nodes[0].height / 2;
  containerOpt.value.targetNodeLeft = nodes[0].left;
  containerOpt.value.targetNodeTop = nodes[0].top;

  // 按连接关系放置其他节点
  const placedNodeIds = new Set([nodes[0].id]);

  // 按照连接关系依次放置其他节点
  let iterations = 0;
  const maxIterations = nodes.length * 10; // 增加迭代次数
  while (placedNodeIds.size < nodes.length && iterations < maxIterations) {
    iterations++;

    nodes.forEach(node => {
      // 如果节点已经放置，跳过
      if (placedNodeIds.has(node.id)) return;

      // 获取已放置的关联节点
      const connectedPlacedNodes = getConnectedPlacedNodes(node.id);

      if (connectedPlacedNodes.length > 0) {
        // 有连接关系的节点放置策略
        let placed = false;

        // 尝试在连接节点周围放置（按优先级排序）
        const directions = [
          { dx: 1, dy: 0 }, // 右
          { dx: -1, dy: 0 }, // 左
          { dx: 0, dy: 1 }, // 下
          { dx: 0, dy: -1 }, // 上
          { dx: 1, dy: 1 }, // 右下
          { dx: -1, dy: 1 }, // 左下
          { dx: 1, dy: -1 }, // 右上
          { dx: -1, dy: -1 } // 左上
        ];

        // 先尝试在连接节点周围放置
        for (const connectedNode of connectedPlacedNodes) {
          if (placed) break;

          for (const dir of directions) {
            // 计算新位置，确保满足最小间距要求
            node.left =
              connectedNode.left +
              (dir.dx * (connectedNode.width + node.width + minSpacing)) / 2;
            node.top =
              connectedNode.top +
              (dir.dy * (connectedNode.height + node.height + minSpacing)) / 2;

            // 调整位置使节点完全位于画布内
            // node.left = Math.max(
            //   minSpacing,
            //   Math.min(options.with - node.width - minSpacing, node.left),
            // )
            // node.top = Math.max(
            //   minSpacing,
            //   Math.min(options.height - node.height - minSpacing, node.top),
            // )

            // 检查是否与其他节点重叠
            if (!checkOverlapWithPlaced(node, node.id)) {
              placedNodeIds.add(node.id);
              placed = true;
              break;
            }
          }

          if (placed) break;
        }

        // 如果周围都无法放置，则使用引力/斥力模型
        if (!placed) {
          // 计算连接节点的中心点
          const centerX =
            connectedPlacedNodes.reduce(
              (sum, n) => sum + n.left + n.width / 2,
              0
            ) / connectedPlacedNodes.length;
          const centerY =
            connectedPlacedNodes.reduce(
              (sum, n) => sum + n.top + n.height / 2,
              0
            ) / connectedPlacedNodes.length;

          // 在中心点周围寻找合适位置
          const angle = Math.random() * Math.PI * 2;
          const distance = 200 + Math.random() * 100;

          node.left = centerX + Math.cos(angle) * distance - node.width / 2;
          node.top = centerY + Math.sin(angle) * distance - node.height / 2;

          // 边界检查
          // node.left = Math.max(
          //   minSpacing,
          //   Math.min(options.with - node.width - minSpacing, node.left),
          // )
          // node.top = Math.max(
          //   minSpacing,
          //   Math.min(options.height - node.height - minSpacing, node.top),
          // )

          // 解决重叠问题

          let attempts = 0;

          let _left = node.left;
          let _top = node.top;
          const moveAngle = Math.random() * Math.PI * 2;
          let moveDistance = 30;
          while (checkOverlapWithPlaced(node, node.id) && attempts < 10) {
            moveDistance += Math.random() * 40;
            // 增加尝试次数
            node.left = _left;
            node.top = _top;
            let angleAttempts = 0;
            while (
              checkOverlapWithPlaced(node, node.id) &&
              angleAttempts < 10
            ) {
              node.left += Math.cos(moveAngle) * moveDistance;
              node.top += Math.sin(moveAngle) * moveDistance;
              angleAttempts++;
            }

            attempts++;
          }

          // 如果最终没有重叠，则标记为已放置
          // if (!checkOverlapWithPlaced(node, node.id)) {
          placedNodeIds.add(node.id);
          placed = true;
          // }
        }
      } else {
        // 没有连接关系的节点，放置在空白区域
        let placed = false;
        let attempts = 0;

        while (!placed && attempts < 10) {
          // 增加尝试次数
          // 在画布范围内随机位置（考虑边界和间距）
          node.left =
            minSpacing +
            Math.random() * (options.with - node.width - minSpacing * 2);
          node.top =
            minSpacing +
            Math.random() * (options.height - node.height - minSpacing * 2);

          if (!checkOverlapWithPlaced(node, node.id)) {
            placedNodeIds.add(node.id);
            placed = true;
          }
          attempts++;
        }

        // 如果随机放置失败，则放置在已放置节点附近
        if (!placed && placedNodeIds.size > 0) {
          // 随机选择一个已放置的节点作为参考
          const placedNodeArray = Array.from(placedNodeIds);
          const referenceNodeId =
            placedNodeArray[Math.floor(Math.random() * placedNodeArray.length)];
          const referenceNode = nodeMap[referenceNodeId];

          // 在参考节点周围寻找位置
          const angle = Math.random() * Math.PI * 2;
          const distance =
            minSpacing + (node.width + referenceNode.width) / 2 + 50;

          node.left =
            referenceNode.left +
            referenceNode.width / 2 +
            Math.cos(angle) * distance -
            node.width / 2;
          node.top =
            referenceNode.top +
            referenceNode.height / 2 +
            Math.sin(angle) * distance -
            node.height / 2;

          // 边界检查
          // node.left = Math.max(
          //   minSpacing,
          //   Math.min(options.with - node.width - minSpacing, node.left),
          // )
          // node.top = Math.max(
          //   minSpacing,
          //   Math.min(options.height - node.height - minSpacing, node.top),
          // )

          // 如果最终没有重叠，则标记为已放置
          if (!checkOverlapWithPlaced(node, node.id)) {
            placedNodeIds.add(node.id);
          }
        }
      }
    });
  }
  // 处理仍未放置的节点（使用网格布局作为最后手段）
  nodes.forEach(node => {
    if (!placedNodeIds.has(node.id)) {
      // 计算网格位置
      const totalPlaced = placedNodeIds.size;
      const gridCols = Math.ceil(Math.sqrt(nodes.length));
      const gridX = totalPlaced % gridCols;
      const gridY = Math.floor(totalPlaced / gridCols);

      // 计算网格单元大小（考虑节点最大尺寸和间距）
      const maxNodeWidth = Math.max(...nodes.map(n => n.width));
      const maxNodeHeight = Math.max(...nodes.map(n => n.height));
      const cellWidth = maxNodeWidth + minSpacing;
      const cellHeight = maxNodeHeight + minSpacing;

      node.left = minSpacing + gridX * cellWidth;
      node.top = minSpacing + gridY * cellHeight;

      // 边界检查
      if (node.left + node.width > options.with - minSpacing) {
        node.left = options.with - node.width - minSpacing;
      }
      if (node.top + node.height > options.height - minSpacing) {
        node.top = options.height - node.height - minSpacing;
      }

      // 解决重叠
      let attempts = 0;
      while (checkOverlapWithPlaced(node, node.id) && attempts < 50) {
        // 增加尝试次数
        node.left += minSpacing;
        if (node.left + node.width > options.with - minSpacing) {
          node.left = minSpacing;
          node.top += minSpacing;
        }
        attempts++;
      }

      placedNodeIds.add(node.id);
    }
  });
  let refMaps = {};
  nodesRef.value.forEach(it => {
    refMaps[it.id] = it;
  });
  // 最终边界检查，确保所有节点都在可视区域内
  nodes.forEach(node => {
    // node.left = Math.max(minSpacing, Math.min(options.with - node.width - minSpacing, node.left))
    // node.top = Math.max(minSpacing, Math.min(options.height - node.height - minSpacing, node.top))

    node.left = closestMultiple(node.left, options.span);
    node.top = closestMultiple(node.top, options.span);

    let nodeRef = refMaps[node.id];
    if (nodeRef) {
      nodeRef.style.left = `${node.left}px`;
      nodeRef.style.top = `${node.top}px`;
    }
  });
}

function getCenterPos(nodes) {
  let leftArray = nodes.map(node => node.left);
  let topArray = nodes.map(node => node.top);

  let minLeft = Math.min(...leftArray);
  let maxLeft = Math.max(...leftArray);
  let minTop = Math.min(...topArray);
  let maxTop = Math.max(...topArray);

  return {
    left: (minLeft + maxLeft + 160) / 2,
    top: (minTop + maxTop + 160) / 2
  };
}

function closestMultiple(n, x) {
  if (x === 0) return n;
  return Math.round(n / x) * x;
}

function ceilMultiple(n, x) {
  if (x === 0) return n;
  return Math.ceil(n / x) * x;
}

function getTextWidth(text) {
  //const text = 'hello world';
  // const font = 'bold 12pt arial';
  const canvas = document.createElement('canvas');
  const context = canvas.getContext('2d');
  //context.font = font;

  const { width } = context.measureText(text);

  return width;
}

function getNodeWidth(node) {
  const canvas = document.createElement('canvas');
  const context = canvas.getContext('2d');

  context.font = '14px Arial';
  const { width } = context.measureText(node.id);
  let maxWidth = width;
  const t2 = context.measureText(node.name);
  if (t2.width > maxWidth) {
    maxWidth = t2.width;
  }

  context.font = '12px Arial';
  node.properties.forEach(item => {
    if (item.isShow) {
      let text = `${item.id}（${item.name}）`;
      const it = context.measureText(text);
      if (it.width > maxWidth) {
        maxWidth = it.width;
      }
    }
  });

  return ceilMultiple(maxWidth, options.span);
}

async function getData() {
  //data.value = jsonData;
  let res = await fetch('/docview/api/entityjson');

  if (res.ok) {
    let json = await res.json();
    data.value = json;
  } else {
    console.log('error', res.status);
    console.error('error', res.statusText);
    return;
  }

  getMenuNodes();
}

function InitJsplumb(callBack) {
  jsPlumb.ready(() => {
    // console.log('jsPlumb ready');
    jsPlumb.setContainer('jsPlumbContainer');
    jsPlumb.importDefaults({
      PaintStyle: {
        strokeWidth: 1,
        stroke: '#3498db',
        joinstyle: 'round'
      },
      DragOptions: { cursor: 'move' },
      Endpoints: [
        ['Dot', { radius: 1 }],
        ['Dot', { radius: 1 }]
      ],
      Connector: [
        'Flowchart',
        { stub: 20, gap: 0, cornerRadius: 8, alwaysRespectStubs: true }
      ], // ['Bezier'],
      EndpointStyles: [{ fill: '#225588' }, { fill: '#558822' }],
      Overlays: [
        ['Arrow', { width: 8, length: 10, location: 1, foldback: 0.1 }]
      ]
    });

    jsPlumbContainerWrapperRef.value.addEventListener(
      'mousewheel',
      mousewheelFun
    );

    jsPlumbContainerWrapperRef.value.addEventListener(
      'mousedown',
      mouseDownFun
    );

    callBack && callBack();
  });
}

function mousewheelFun(e) {
  let zoom = containerOpt.value.zoom;
  // if (e.ctrlKey) {
  if (e.wheelDeltaY > 0) {
    if (zoom < 5) {
      zoom += 0.1;
    }
  } else {
    if (zoom > 0.4) {
      zoom -= 0.1;
    }
  }
  zoomFun(zoom);
  e.preventDefault();
  e.stopPropagation();
  //  }
}

const zoomShow = computed(() => {
  return containerOpt.value.zoom * 10 * 10 + '%';
});
function zoomFun(scale) {
  scale = Math.round(scale * 10) / 10;
  containerOpt.value.zoom = scale;
  jsPlumb.setZoom(scale);
  // console.log('zoom', scale)
}

function mouseDownFun(e) {
  //if (e.ctrlKey) {
  jsPlumbContainerRef.value.__mouseIsDown = true;
  jsPlumbContainerRef.value.__mouseX = e.clientX;
  jsPlumbContainerRef.value.__mouseY = e.clientY;
  jsPlumbContainerRef.value.__x = containerOpt.value.left;
  jsPlumbContainerRef.value.__y = containerOpt.value.top;
  jsPlumbContainerWrapperRef.value.removeEventListener(
    'mousemove',
    mouseMoveFun
  );

  document.removeEventListener('mouseup', mouseUpFun);
  document.addEventListener('mouseup', mouseUpFun);
  jsPlumbContainerWrapperRef.value.addEventListener('mousemove', mouseMoveFun);
  //console.log('mousedown', e)
  //}
}
function mouseUpFun(e) {
  // console.log('mouseup')
  if (jsPlumbContainerRef.value.__mouseIsDown) {
    let x = e.clientX - jsPlumbContainerRef.value.__mouseX;
    let y = e.clientY - jsPlumbContainerRef.value.__mouseY;
    // console.log('move', x, y)
    x = closestMultiple(x, options.span);
    y = closestMultiple(y, options.span);
    containerOpt.value.left = jsPlumbContainerRef.value.__x + x;
    containerOpt.value.top = jsPlumbContainerRef.value.__y + y;
  }
  jsPlumbContainerRef.value.__mouseIsDown = false;
  jsPlumbContainerWrapperRef.value.removeEventListener(
    'mousemove',
    mouseMoveFun
  );
  document.removeEventListener('mouseup', mouseUpFun);
}
function mouseMoveFun(e) {
  if (jsPlumbContainerRef.value.__mouseIsDown) {
    let x = e.clientX - jsPlumbContainerRef.value.__mouseX;
    let y = e.clientY - jsPlumbContainerRef.value.__mouseY;
    // console.log('move', x, y)
    containerOpt.value.left = jsPlumbContainerRef.value.__x + x;
    containerOpt.value.top = jsPlumbContainerRef.value.__y + y;
  }
}

function moveTo(x, y) {
  containerOpt.value.left = x;
  containerOpt.value.top = y;
}

function moverToCenter() {
  if (containerOpt.value.targetNode) {
    let viewWidth = jsPlumbContainerWrapperRef.value.offsetWidth;
    let viewHeight = jsPlumbContainerWrapperRef.value.offsetHeight;

    let center = containerOpt.value.targetNode;
    let left = viewWidth / 2 - center.left - center.width / 2;
    let top = viewHeight / 2 - center.top - center.height / 2;

    left = closestMultiple(left, options.span);
    top = closestMultiple(top, options.span);

    containerOpt.value.left = left;
    containerOpt.value.top = top;
    zoomFun(1);
  } else {
    let viewWidth = jsPlumbContainerWrapperRef.value.offsetWidth;
    let viewHeight = jsPlumbContainerWrapperRef.value.offsetHeight;

    let center = getCenterPos(json.value.nodes);
    let left = viewWidth / 2 - center.left;
    let top = viewHeight / 2 - center.top;

    left = closestMultiple(left, options.span);
    top = closestMultiple(top, options.span);

    containerOpt.value.left = left;
    containerOpt.value.top = top;
  }

  // if (containerOpt.value.targetNodeLeft) {
  //   let viewWidth = jsPlumbContainerWrapperRef.value.offsetWidth
  //   let viewHeight = jsPlumbContainerWrapperRef.value.offsetHeight

  //   let left = viewWidth / 2 - containerOpt.value.targetNodeLeft
  //   let top = viewHeight / 2 - containerOpt.value.targetNodeTop
  //   left = closestMultiple(left, options.span)
  //   top = closestMultiple(top, options.span)

  //   containerOpt.value.left = left
  //   containerOpt.value.top = top
  // } else {
  //   containerOpt.value.left = containerOpt.value.centerLeft
  //   containerOpt.value.top = containerOpt.value.centerTop
  // }
}

function renderJsplumb(selectJson) {
  jsPlumb.deleteEveryConnection();
  jsPlumb.deleteEveryEndpoint();
  json.value = selectJson;
  nextTick(() => {
    initNodes(json.value.nodes);

    let viewWidth = jsPlumbContainerWrapperRef.value.offsetWidth;
    let viewHeight = jsPlumbContainerWrapperRef.value.offsetHeight;

    let center = getCenterPos(json.value.nodes);
    let left = viewWidth / 2 - center.left;
    let top = viewHeight / 2 - center.top;

    left = closestMultiple(left, options.span);
    top = closestMultiple(top, options.span);

    containerOpt.value.left = left;
    containerOpt.value.top = top;
    // jsPlumbContainerRef.value.style.transform = `translate(${left}px, ${top}px)`
    nextTick(() => {
      // jsPlumb.empty('jsPlumbContainer')

      // jsPlumb.addEndpoint('sourceElement', {
      //   anchors: ['Left'],
      //   isSource: true,
      //   isTarget: true,
      // })

      json.value.nodes.forEach(node => {
        jsPlumb.draggable(node.id, {
          //containment: '#jsPlumbContainerWrapper',
          grid: [options.span, options.span],
          stop: function (event) {
            // console.log('dddr', event, ui)
            if (event.pos && event.pos.length == 2) {
              node.left = event.pos[0];
              node.top = event.pos[1];
            }
          }
        });
      });

      json.value.connects.forEach(connect => {
        let overlays = [];
        if (connect.label) {
          overlays.push([
            'Label',
            {
              label: `${connect.label}`,
              cssClass: 'entity-view-line-label',
              labelStyle: { color: '#3498db' }
            }
          ]);
        }
        jsPlumb.connect({
          source: connect.source,
          target: connect.target,
          // connector: ['Flowchart', { stub: 20, gap: 0, cornerRadius: 8, alwaysRespectStubs: true }], // ['Bezier'],
          anchor: ['Left', 'Right'],
          overlays: overlays
          // paintStyle: { stroke: 'lightgray', strokeWidth: 1 },
          // endpointStyle: {
          //   fill: 'lightgray',
          //   outlineStroke: 'darkgray',
          //   outlineWidth: 8,
          // },
        });
      });

      //jsPlumb.repaintEverything()
    });
  });
}

onMounted(async () => {
  await getData();
  InitJsplumb(function () {
    //renderJsplumb(data.value)
  });
});

onUnmounted(() => {
  if (jsPlumbContainerRef.value) {
    jsPlumbContainerRef.value.removeEventListener('scroll');
    jsPlumbContainerRef.value.removeEventListener('mousedown');
  }

  jsPlumb.destroy();
});

function filterNode(node) {
  containerOpt.value.targetNode = node;
  json.value = getActiveRelationNodes(node);

  renderJsplumb(json.value);
}

const searchValue = ref('');

const menuNodes = ref([]);

function getMenuNodes() {
  let nodes = [];
  if (searchValue.value) {
    let searchstr = searchValue.value.toLowerCase();
    nodes = data.value.nodes.filter(
      f =>
        f.name.toLowerCase().indexOf(searchstr) > -1 ||
        (f.des || '').toLowerCase().indexOf(searchstr) > -1
    );
  } else {
    nodes = data.value.nodes;
  }
  const tagsSet = new Set();
  let tags = {};

  nodes.forEach(node => {
    node.tag = node.tag || '   ';
    if (node.tag) {
      if (tagsSet.has(node.tag)) {
        tags[node.tag].push(node);
      } else {
        tagsSet.add(node.tag);
        tags[node.tag] = [];
        tags[node.tag].push(node);
      }
    }
  });
  let tree = [];
  let noneTag = [];
  for (const [key, element] of Object.entries(tags)) {
    if (key === '   ') {
      noneTag = element;
    } else {
      tree.push({
        isTag: true,
        tag: key,
        children: element
      });
    }
  }
  tree.push(...noneTag);

  menuNodes.value = tree;
}

function clickTag(item) {
  item._isOpen = !item._isOpen;
}

function getActiveRelationNodes(inputNode) {
  // 如果没有传入节点或数据为空，返回空结果
  if (!inputNode || !data.value || !data.value.nodes || !data.value.connects) {
    return { nodes: [], connects: [] };
  }

  // 存储所有相关的节点ID
  const relatedNodeIds = new Set();
  relatedNodeIds.add(inputNode.id); // 添加输入节点本身

  // 存储所有相关的连接
  const relatedConnects = new Set();

  // 构建节点关系图，方便查找
  const nodeRelations = {};
  data.value.connects.forEach(connect => {
    const sourceNodeId = connect.sourceNode;
    const targetNodeId = connect.targetNode;

    if (!nodeRelations[sourceNodeId]) nodeRelations[sourceNodeId] = new Set();
    if (!nodeRelations[targetNodeId]) nodeRelations[targetNodeId] = new Set();

    nodeRelations[sourceNodeId].add(targetNodeId);
    nodeRelations[targetNodeId].add(sourceNodeId);
  });

  // 递归查找所有关联的节点
  function findRelatedNodes(nodeId, level) {
    if (level >= options.relationLevel) {
      return;
    }
    level++;
    // 获取直接关联的节点
    const directlyConnectedNodes = nodeRelations[nodeId] || new Set();

    directlyConnectedNodes.forEach(connectedNodeId => {
      // 如果这个节点还没有被添加到关联节点集合中
      if (!relatedNodeIds.has(connectedNodeId)) {
        relatedNodeIds.add(connectedNodeId);

        // 查找与这个节点相关的连接
        data.value.connects.forEach(connect => {
          const sourceNodeId = connect.sourceNode;
          const targetNodeId = connect.targetNode;

          if (
            (sourceNodeId === nodeId && targetNodeId === connectedNodeId) ||
            (sourceNodeId === connectedNodeId && targetNodeId === nodeId)
          ) {
            relatedConnects.add(connect);
          }
        });

        // 递归查找这个节点的关联节点
        findRelatedNodes(connectedNodeId, level);
      } else {
        // 即使节点已经被添加，也要检查是否存在与之相关的连接
        data.value.connects.forEach(connect => {
          const sourceNodeId = connect.sourceNode;
          const targetNodeId = connect.targetNode;

          if (
            (sourceNodeId === nodeId && targetNodeId === connectedNodeId) ||
            (sourceNodeId === connectedNodeId && targetNodeId === nodeId)
          ) {
            relatedConnects.add(connect);
          }
        });
      }
    });
  }

  // 开始递归查找
  findRelatedNodes(inputNode.id, 0);

  // 查找所有相关的连接（包括直接和间接的）
  data.value.connects.forEach(connect => {
    const sourceNodeId = connect.sourceNode;
    const targetNodeId = connect.targetNode;

    // 如果连接的两端节点都在关联节点集合中，则添加这条连接
    if (relatedNodeIds.has(sourceNodeId) && relatedNodeIds.has(targetNodeId)) {
      relatedConnects.add(connect);
    }
  });

  // 根据ID获取完整的节点对象
  const relatedNodes = data.value.nodes.filter(node =>
    relatedNodeIds.has(node.id)
  );

  // 转换Set为数组并返回结果
  return {
    nodes: relatedNodes,
    connects: Array.from(relatedConnects)
  };
}

const detailNode = ref({});

function activeDetailNode(node) {
  nextTick(() => {
    detailNode.value = node;
  });
}
</script>

<template>
  <div class="entity-view-main">
    <div class="entity-view-menutoolbar">
      <div>
        <el-input
          size="small"
          style="width: 200px"
          v-model="searchValue"
          type="text"
          placeholder="搜索"
          @keyup.enter="getMenuNodes()" />

        <el-button type="primary" size="small" @click="getMenuNodes()"
          ><IconSearch
        /></el-button>
      </div>
      <div class="entity-view-list">
        <ul>
          <template v-for="(item, index) in menuNodes" :key="item">
            <template v-if="item.isTag">
              <li class="entity-view-list-tag-wrapper">
                <div class="entity-view-list-tag" @click="clickTag(item)">
                  <span class="entity-view-list-tag-icon"
                    ><IconArrowDown v-if="item._isOpen" /><IconArrowRight
                      v-else />
                  </span>

                  <span class="entity-view-list-tag-title">{{ item.tag }}</span>
                </div>
                <ul :class="item._isOpen ? 'open' : ''">
                  <li
                    v-for="node in item.children"
                    :key="node"
                    @click="filterNode(node)">
                    <div class="entity-view-list-item">
                      {{ node.name }}
                      <div class="entity-view-nameinfo" v-if="node.des">
                        {{ node.des }}
                      </div>
                    </div>
                  </li>
                </ul>
              </li>
            </template>
            <template v-else>
              <li class="entity-view-list-li" @click="filterNode(item)">
                <div class="entity-view-list-item">
                  {{ item.name }}
                  <div class="entity-view-nameinfo" v-if="item.des">
                    {{ item.des }}
                  </div>
                </div>
              </li>
            </template>
          </template>
        </ul>
      </div>
    </div>
    <div
      class="entity-view-wrapper"
      :style="{ width: `${options.viewWith}`, height: `100%` }"
      id="jsPlumbContainerWrapper"
      ref="jsPlumbContainerWrapperRef">
      <div class="entity-view-toolbar">
        <div @click="zoomFun(1)" class="entity-view-toolbar-item">
          {{ zoomShow }}
        </div>
        <div class="entity-view-toolbar-item" @click="moverToCenter()">
          <IconCenter />
        </div>
      </div>
      <div
        class="entity-view-container"
        id="jsPlumbContainer"
        ref="jsPlumbContainerRef">
        <div
          :id="node.id"
          ref="nodesRef"
          @click="activeDetailNode(node)"
          v-for="node in json.nodes"
          :key="node"
          :style="{
            width: `${node.width}px`,
            height: `${node.height}px`
          }"
          class="entity-view">
          <div
            class="entity-view-title"
            :class="node.des ? 'entity-view-title__hasSub' : ''">
            {{ node.name }}
            <div class="entity-view-nameinfo" v-if="node.des">
              ({{ node.des }})
            </div>
          </div>

          <div class="entity-view-property-container">
            <template v-for="property in node.properties" :key="property">
              <div
                v-if="property.isShow"
                class="entity-view-property"
                :id="`${node.id}.${property.id}`">
                {{ property.name
                }}<span class="entity-view-property-name" v-if="property.des"
                  >( {{ property.des }})</span
                >
              </div>
            </template>
          </div>
        </div>
      </div>
    </div>
    <div class="entity-view-detail-wrapper">
      <div class="entity-view-detail">
        <div
          class="entity-view-title"
          :class="
            detailNode.des || detailNode.schema
              ? 'entity-view-title__hasSub'
              : ''
          ">
          {{ detailNode.name }}
          <div
            class="entity-view-nameinfo"
            v-if="detailNode.des || detailNode.schema">
            <span v-if="detailNode.schema">[{{ detailNode.schema }}]</span>
            <span v-if="detailNode.des"> ({{ detailNode.des }}) </span>
          </div>
        </div>

        <div class="entity-view-property-container">
          <template v-for="property in detailNode.properties" :key="property">
            <div class="entity-view-property">
              {{ property.name
              }}<span class="entity-view-property-name" v-if="property.des"
                >( {{ property.des }})</span
              >
              <div class="entity-view-property-subdes">
                {{ property.subDes }}
              </div>
            </div>
          </template>
        </div>
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.entity-view-main {
  width: 100%;
  display: flex;
  font-size: 14px;
  height: 100%;
  padding: 20px 20px;
  height: calc(100vh);
  .entity-view-nameinfo {
    color: #999;
    text-wrap: nowrap;
  }

  .entity-view-menutoolbar {
    flex: 0 0 300px;
  }

  .entity-view-list {
    margin-top: 20px;
    max-height: calc(100% - 46px);
    min-width: 200px;
    max-width: 300px;

    overflow-x: hidden;
    overflow-y: auto;
    border: 1px solid #e0e0e0;
    border-radius: 8px;
    background-color: #fafafa;

    .entity-view-list-tag-wrapper {
      > ul {
        display: none;
        &.open {
          display: block;
        }
      }

      .entity-view-list-tag {
        padding: 4px 2px;
        cursor: pointer;
        border-bottom: 1px solid #eee;
        transition: all 0.2s ease;
        font-size: 14px;
        color: #333;
        &:last-child {
          border-bottom: none;
        }
        &:hover {
          background-color: #e3f2fd;
          color: #1976d2;
          transform: translateX(5px);
        }
        .entity-view-list-tag-icon {
          margin: 0 5px;
        }
      }
    }

    & > ul {
      list-style: none;
      padding-inline-start: 0;
      & > li {
        ul {
          padding-inline-start: 10px;
        }
      }
      & > .entity-view-list-li {
        padding-inline-start: 2px;
      }
    }

    .entity-view-list-item {
      padding: 4px 8px;
      cursor: pointer;
      border-bottom: 1px solid #eee;
      transition: all 0.2s ease;
      font-size: 14px;
      color: #333;

      .entity-view-nameinfo {
        font-size: 12px;
      }

      &:last-child {
        border-bottom: none;
      }

      &:hover {
        background-color: #e3f2fd;
        color: #1976d2;
        transform: translateX(5px);
      }

      &:active {
        background-color: #bbdefb;
      }
    }

    // 自定义滚动条样式
    &::-webkit-scrollbar {
      width: 6px;
    }

    &::-webkit-scrollbar-track {
      background: #f1f1f1;
      border-radius: 10px;
    }

    &::-webkit-scrollbar-thumb {
      background: #c1c1c1;
      border-radius: 10px;

      &:hover {
        background: #a1a1a1;
      }
    }
  }
}
.entity-view-wrapper {
  position: relative;
  border-radius: 10px;
  overflow: hidden;
  margin: 0 10px;
  // background-color: rgb(255, 255, 255);
  box-shadow: 0 0 10px rgb(214, 214, 214);
  background-image: linear-gradient(
      to right,
      #f3f3f3 1px,
      transparent 1px,
      transparent 20px
    ),
    linear-gradient(to bottom, #f3f3f3 1px, transparent 1px, transparent 20px);
  background-size: 20px 20px;
  .entity-view-toolbar {
    position: absolute;
    padding: 0 10px;
    box-shadow: 0 0 10px rgb(224, 224, 224);
    height: 30px;
    min-width: 200px;
    border-radius: 10px;
    bottom: 10px;
    left: 50%;
    background-color: white;
    transform: translateX(-50%);
    z-index: 10;
    display: flex;
    justify-content: center;
    align-items: center;
    .entity-view-toolbar-item {
      cursor: pointer;
      padding: 0 4px;
      // border-radius: 4px;
      height: 100%;
      display: flex;
      justify-content: center;
      align-items: center;
      &:hover {
        background-color: #f1f1f1;
      }
    }
  }
  .entity-view-container {
    width: 2000px;
    height: 2000px;
    position: relative;
    // transform: translate(40%, 40%);
    .entity-view {
      position: absolute;
      box-shadow: 0 0 10px rgb(214, 214, 214);
      border-radius: 6px;
      background-color: white;
      min-width: 120px;
      overflow: hidden;
      transition: transform 0.2s ease;
      &:hover {
        box-shadow: 0 0 4px #2b85c2;
        z-index: 2;
        transform: scale(1.03);
      }

      .entity-view-title {
        padding: 5px 20px;
        height: 40px;
        background-color: #f7f7f7;
        cursor: move;
        &.entity-view-title__hasSub {
          height: 50px;
        }
        .entity-view-nameinfo {
          font-size: 12px;
        }
      }
      .entity-view-property-container {
        // height: 0;
        // overflow: hidden;
        display: flex;
        flex-direction: column;
        .entity-view-property {
          padding: 2px 20px;
          font-size: 12px;
          height: 20px;
          text-wrap: nowrap;
          &:hover {
            background-color: #f7f7f7;
          }
          .entity-view-property-name {
            color: #999;
          }
        }
      }
    }
    :deep() {
      .entity-view-line-label {
        padding: 2px 4px;
        color: #3498db;
        background-color: white;
      }
    }
  }
}

.entity-view-detail-wrapper {
  width: 300px;
  flex: 0 0 300px;
  height: 100%;
  box-sizing: content-box;

  .entity-view-detail {
    width: 100%;
    height: 100%;

    box-shadow: 0 0 4px rgb(192, 192, 192);
    overflow: hidden;

    border-radius: 10px;
  }

  .entity-view-title {
    width: 100%;
    padding: 5px 20px;
    height: 40px;
    background-color: #f7f7f7;
    &.entity-view-title__hasSub {
      height: 50px;
    }
    .entity-view-nameinfo {
      font-size: 12px;
    }
  }
  .entity-view-property-container {
    height: calc(100% - 50px);
    overflow: hidden;
    overflow-y: auto;
    // height: 0;
    // overflow: hidden;
    display: flex;
    flex-direction: column;
    .entity-view-property {
      padding: 2px 20px;
      font-size: 12px;
      height: 20px;
      text-wrap: nowrap;
      &:hover {
        background-color: #f7f7f7;
      }
      .entity-view-property-name {
        color: #999;
      }
      .entity-view-property-subdes {
        background-color: #f8f8f8;
        color: #999;
        text-wrap: wrap;
        font-size: 1em;
        padding-left: 1em;
        border-radius: 6px;
      }
    }
  }
}
</style>
